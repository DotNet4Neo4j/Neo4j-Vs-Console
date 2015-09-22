namespace Anabranch.Neo4JConsolePackage.AutoCompleteBehavior
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Anabranch.Neo4JConsolePackage.Extensions;

    /// <summary>
    ///     This is taken from https://github.com/Nimgoble/WPFTextBoxAutoComplete/ - it's added as code as extensions to VS
    ///     require signed dlls to work.
    /// </summary>
    public static class AutoCompleteBehavior
    {
        private static readonly TextChangedEventHandler OnTextChanged = TextBox_OnTextChanged;
        private static readonly KeyEventHandler OnPreviewKeyDown = TextBox_OnPreviewKeyDown;

        /// <summary>Used for moving the caret to the end of the suggested auto-completion text.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TextBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            var tb = e.OriginalSource as TextBox;
            if (tb == null)
                return;

            //If we pressed enter and if the selected text goes all the way to the end, move our caret position to the end
            if (tb.SelectionLength > 0) // && (tb.SelectionStart + tb.SelectionLength == tb.Text.Length))
            {
                var caretShouldgoTo = tb.SelectionStart + tb.SelectionLength;
                tb.SelectionStart = tb.CaretIndex = caretShouldgoTo;
                //tb.SelectionStart = tb.CaretIndex = tb.Text.Length;
                tb.SelectionLength = 0;
            }

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift)
            {
                var caretLocation = tb.CaretIndex;
                tb.TextChanged -= OnTextChanged;
                tb.Text = tb.Text.Insert(tb.CaretIndex, Environment.NewLine);
                tb.CaretIndex = tb.SelectionStart = caretLocation + 2;
                tb.TextChanged += OnTextChanged;
            }
        }

        private static bool ShouldInsertClosingCharacter(char enteredCharacter, out char closingCharacter)
        {
            switch (enteredCharacter)
            {
                case '`':
                    closingCharacter = '`';
                    return true;
                case '(':
                    closingCharacter = ')';
                    return true;
                case '{':
                    closingCharacter = '}';
                    return true;
                case '[':
                    closingCharacter = ']';
                    return true;
                case '\'':
                case '\"':
                    closingCharacter = enteredCharacter;
                    return true;
                default:
                    closingCharacter = ' ';
                    return false;
            }
        }

        /// <summary>Search for auto-completion suggestions.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (e.Changes.Any(change => change.RemovedLength > 0) && e.Changes.Any(change => change.AddedLength > 0) == false)
                return;

            var tb = e.OriginalSource as TextBox;
            if (tb == null)
                return;

            var labels = GetAutoCompleteLabelsSource(tb).ToList();
            var relationships = GetAutoCompleteRelationshipsSource(tb).ToList();
            var values = GetAutoCompleteItemsSource(tb).ToList();
            //No reason to search if we don't have any values.
            if (values.IsNullOrEmpty() && relationships.IsNullOrEmpty() && labels.IsNullOrEmpty())
                return;

            //No reason to search if there's nothing there.
            if (string.IsNullOrEmpty(tb.Text))
                return;

            var comparer = GetAutoCompleteStringComparison(tb);
            //Do search and changes here.

            var doesContainSpaces = tb.Text.Contains(" ");
            var doesContainColons = tb.Text.Contains(":");
            var doesContainNewLines = tb.Text.Contains(Environment.NewLine);
            var spaceBeforeCaretIndex = 0;
            var colonBeforeCaretIndex = 0;
            var newlineBeforeCaretIndex = 0;
            if (doesContainSpaces)
            {
                spaceBeforeCaretIndex = tb.Text.Substring(0, tb.CaretIndex).LastIndexOf(" ", StringComparison.InvariantCultureIgnoreCase) + 1;
                if (spaceBeforeCaretIndex <= 0)
                    spaceBeforeCaretIndex = 0;
            }
            if (doesContainColons)
            {
                colonBeforeCaretIndex = tb.Text.Substring(0, tb.CaretIndex).LastIndexOf(":", StringComparison.InvariantCultureIgnoreCase) + 1;
                if (colonBeforeCaretIndex <= 0)
                    colonBeforeCaretIndex = 0;
            }
            if (doesContainNewLines)
            {
                newlineBeforeCaretIndex = tb.Text.Substring(0, tb.CaretIndex).LastIndexOf(Environment.NewLine, StringComparison.InvariantCultureIgnoreCase) + (Environment.NewLine.Length);
                if (newlineBeforeCaretIndex <= 0)
                    newlineBeforeCaretIndex = 0;
            }

            var valueToStartAt = Math.Max(newlineBeforeCaretIndex, Math.Max(spaceBeforeCaretIndex, colonBeforeCaretIndex));

            var lastCharPressed = tb.Text[tb.CaretIndex - 1];
            char closingChar;
            if (ShouldInsertClosingCharacter(lastCharPressed, out closingChar))
            {
                tb.TextChanged -= OnTextChanged;
                var cIndex = tb.CaretIndex;
                tb.Text = tb.Text.Insert(tb.CaretIndex, closingChar.ToString());
                tb.CaretIndex = cIndex;
                tb.TextChanged += OnTextChanged;
            }


            var textToLookFor = tb.Text.Substring(0, tb.CaretIndex).Substring(valueToStartAt);
            var lengthOfText = textToLookFor.Length;
            var initialCaretIndex = tb.CaretIndex;

            string match;
            if (CypherEvaluator.WasLastSignificantCharARelationshipLabel(tb.Text.Substring(0, tb.CaretIndex)))
                match = GetFirstMatch(relationships, lengthOfText, textToLookFor, comparer);
            else if (CypherEvaluator.WasLastSignificantCharANodeLabel(tb.Text.Substring(0, tb.CaretIndex)))
                match = GetFirstMatch(labels, lengthOfText, textToLookFor, comparer);
            else
                match = GetFirstMatch(values, lengthOfText, textToLookFor, comparer);

            //Nothing.  Leave 'em alone
            if (string.IsNullOrEmpty(match) || textToLookFor.Length == 0)
                return;

            tb.TextChanged -= OnTextChanged;
            //tb.Text = match;
            tb.Text = tb.Text.Insert(tb.CaretIndex, match.Substring(textToLookFor.Length, match.Length - textToLookFor.Length));

            tb.CaretIndex = lengthOfText;
            tb.SelectionStart = initialCaretIndex;
            tb.SelectionLength = (match.Length - lengthOfText);
            tb.TextChanged += OnTextChanged;
        }

        private static string GetFirstMatch(ICollection<string> collection, int lengthOfText, string subString, StringComparison comparer)
        {
            var match = (collection
                .Where(subvalue => subvalue.Length >= lengthOfText)
                .Where(value => value.Substring(0, lengthOfText)
                    .Equals(subString, comparer)))
                .FirstOrDefault();

            return match;
        }

        #region Dependency Properties

        /// <summary>The collection to search for matches from.</summary>
        public static readonly DependencyProperty AutoCompleteLabelsSource =
            DependencyProperty.RegisterAttached
                (
                    nameof(AutoCompleteLabelsSource),
                    typeof (IEnumerable<string>),
                    typeof (AutoCompleteBehavior),
                    new UIPropertyMetadata(null, OnAutoCompleteLabelsSource)
                );

        public static readonly DependencyProperty AutoCompleteRelationshipsSource =
            DependencyProperty.RegisterAttached
                (
                    nameof(AutoCompleteRelationshipsSource),
                    typeof (IEnumerable<string>),
                    typeof (AutoCompleteBehavior),
                    new UIPropertyMetadata(null, OnAutoCompleteRelationshipsSource)
                );


        /// <summary>The collection to search for matches from.</summary>
        public static readonly DependencyProperty AutoCompleteItemsSource =
            DependencyProperty.RegisterAttached
                (
                    nameof(AutoCompleteItemsSource),
                    typeof (IEnumerable<string>),
                    typeof (AutoCompleteBehavior),
                    new UIPropertyMetadata(null, OnAutoCompleteItemsSource)
                );

        /// <summary>Whether or not to ignore case when searching for matches.</summary>
        public static readonly DependencyProperty AutoCompleteStringComparison =
            DependencyProperty.RegisterAttached
                (
                    "AutoCompleteStringComparison",
                    typeof (StringComparison),
                    typeof (AutoCompleteBehavior),
                    new UIPropertyMetadata(StringComparison.Ordinal)
                );

        #endregion Dependency Properties

        #region Labels Source

        public static IEnumerable<string> GetAutoCompleteLabelsSource(DependencyObject obj)
        {
            var objRtn = obj.GetValue(AutoCompleteLabelsSource);
            return objRtn as IEnumerable<string>;
        }

        public static void SetAutoCompleteLabelsSource(DependencyObject obj, IEnumerable<string> value)
        {
            obj.SetValue(AutoCompleteLabelsSource, value);
        }

        private static void OnAutoCompleteLabelsSource(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //            var tb = sender as TextBox;
            //            if (tb == null)
            //                return;
            //
            //            //If we're being removed, remove the callbacks
            //            if (e.NewValue == null)
            //            {
            //                tb.TextChanged -= OnTextChanged;
            //                tb.KeyDown -= OnPreviewKeyDown;
            //            }
            //            else
            //            {
            //                //New source.  Add the callbacks
            //                tb.TextChanged += OnTextChanged;
            //                tb.KeyDown += OnPreviewKeyDown;
            //            }
        }

        #endregion Labels Source

        #region Relationships Source

        public static IEnumerable<string> GetAutoCompleteRelationshipsSource(DependencyObject obj)
        {
            var objRtn = obj.GetValue(AutoCompleteRelationshipsSource);
            return objRtn as IEnumerable<string>;
        }

        public static void SetAutoCompleteRelationshipsSource(DependencyObject obj, IEnumerable<string> value)
        {
            obj.SetValue(AutoCompleteRelationshipsSource, value);
        }


        private static void OnAutoCompleteRelationshipsSource(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //            var tb = sender as TextBox;
            //            if (tb == null)
            //                return;
            //
            //            //If we're being removed, remove the callbacks
            //            if (e.NewValue == null)
            //            {
            //                tb.TextChanged -= OnTextChanged;
            //                tb.KeyDown -= OnPreviewKeyDown;
            //            }
            //            else
            //            {
            //                //New source.  Add the callbacks
            //                tb.TextChanged += OnTextChanged;
            //                tb.KeyDown += OnPreviewKeyDown;
            //            }
        }

        #endregion Relationships Source

        #region Items Source

        public static IEnumerable<string> GetAutoCompleteItemsSource(DependencyObject obj)
        {
            var objRtn = obj.GetValue(AutoCompleteItemsSource);
            return objRtn as IEnumerable<string>;
        }

        public static void SetAutoCompleteItemsSource(DependencyObject obj, IEnumerable<string> value)
        {
            obj.SetValue(AutoCompleteItemsSource, value);
        }

        private static void OnAutoCompleteItemsSource(object sender, DependencyPropertyChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (tb == null)
                return;

            //If we're being removed, remove the callbacks
            if (e.NewValue == null)
            {
                tb.TextChanged -= OnTextChanged;
                tb.KeyDown -= OnPreviewKeyDown;
            }
            else
            {
                //New source.  Add the callbacks
                tb.TextChanged += OnTextChanged;
                tb.KeyDown += OnPreviewKeyDown;
            }
        }

        #endregion

        #region String Comparison

        public static StringComparison GetAutoCompleteStringComparison(DependencyObject obj)
        {
            return (StringComparison) obj.GetValue(AutoCompleteStringComparison);
        }

        public static void SetAutoCompleteStringComparison(DependencyObject obj, StringComparison value)
        {
            obj.SetValue(AutoCompleteStringComparison, value);
        }

        #endregion
    }
}