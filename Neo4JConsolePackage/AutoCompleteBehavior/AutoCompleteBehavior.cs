namespace Anabranch.Neo4JConsolePackage.AutoCompleteBehavior
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    /// <summary>
    ///     This is taken from https://github.com/Nimgoble/WPFTextBoxAutoComplete/ - it's added as code as extensions to VS
    ///     require signed dlls to work.
    /// </summary>
    public static class AutoCompleteBehavior
    {
        private static readonly TextChangedEventHandler onTextChanged = OnTextChanged;
        private static readonly KeyEventHandler onKeyDown = OnPreviewKeyDown;

        /// <summary>
        ///     The collection to search for matches from.
        /// </summary>
        public static readonly DependencyProperty AutoCompleteItemsSource =
            DependencyProperty.RegisterAttached
                (
                    "AutoCompleteItemsSource",
                    typeof (IEnumerable<string>),
                    typeof (AutoCompleteBehavior),
                    new UIPropertyMetadata(null, OnAutoCompleteItemsSource)
                );

        /// <summary>
        ///     Whether or not to ignore case when searching for matches.
        /// </summary>
        public static readonly DependencyProperty AutoCompleteStringComparison =
            DependencyProperty.RegisterAttached
                (
                    "AutoCompleteStringComparison",
                    typeof (StringComparison),
                    typeof (AutoCompleteBehavior),
                    new UIPropertyMetadata(StringComparison.Ordinal)
                );

        /// <summary>
        ///     Used for moving the caret to the end of the suggested auto-completion text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            var tb = e.OriginalSource as TextBox;
            if (tb == null)
                return;

            //If we pressed enter and if the selected text goes all the way to the end, move our caret position to the end
            if (tb.SelectionLength > 0 && (tb.SelectionStart + tb.SelectionLength == tb.Text.Length))
            {
                tb.SelectionStart = tb.CaretIndex = tb.Text.Length;
                tb.SelectionLength = 0;
            }
        }

        /// <summary>
        ///     Search for auto-completion suggestions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if( (from change in e.Changes where change.RemovedLength > 0 select change).Any() && (from change in e.Changes where change.AddedLength > 0 select change).Any() == false)
                return;

            var tb = e.OriginalSource as TextBox;
            if (sender == null || tb == null)
                return;

            var values = GetAutoCompleteItemsSource(tb);
            //No reason to search if we don't have any values.
            if (values == null)
                return;

            //No reason to search if there's nothing there.
            if (string.IsNullOrEmpty(tb.Text))
                return;

            var comparer = GetAutoCompleteStringComparison(tb);
            //Do search and changes here.

            var doesContainSpaces = tb.Text.Contains(" ");
            var spaceBeforeCaretIndex = 0;
            if (doesContainSpaces)
            {
                spaceBeforeCaretIndex = tb.Text.Substring(0, tb.CaretIndex).LastIndexOf(" ", StringComparison.InvariantCultureIgnoreCase) + 1;
                if (spaceBeforeCaretIndex <= 0)
                    spaceBeforeCaretIndex = 0;
            }

            var textToLookFor = tb.Text.Substring(0, tb.CaretIndex).Substring(spaceBeforeCaretIndex);
            var lengthOfText = textToLookFor.Length;
            var initialCaretIndex = tb.CaretIndex;

            var match = (values
                .Where(subvalue => subvalue.Length >= lengthOfText)
                .Where(value => value.Substring(0, lengthOfText)
                .Equals(textToLookFor, comparer)))
                .FirstOrDefault();
            //            var match = (values.Where(subvalue => subvalue.Length >= textLength).Where(value => value.Substring(0, textLength).Equals(tb.Text, comparer))).FirstOrDefault();

            //Nothing.  Leave 'em alone
            if (string.IsNullOrEmpty(match))
                return;

            tb.TextChanged -= onTextChanged;
            //tb.Text = match;
            tb.Text = tb.Text.Insert(tb.CaretIndex, match.Substring(textToLookFor.Length, match.Length - textToLookFor.Length));

            tb.CaretIndex = lengthOfText;
            tb.SelectionStart = initialCaretIndex;
            tb.SelectionLength = (match.Length - lengthOfText);
            tb.TextChanged += onTextChanged;
        }

        #region Items Source

        public static IEnumerable<string> GetAutoCompleteItemsSource(DependencyObject obj)
        {
            var objRtn = obj.GetValue(AutoCompleteItemsSource);
            if (objRtn is IEnumerable<string>)
                return (objRtn as IEnumerable<string>);

            return null;
        }

        public static void SetAutoCompleteItemsSource(DependencyObject obj, IEnumerable<string> value)
        {
            obj.SetValue(AutoCompleteItemsSource, value);
        }

        private static void OnAutoCompleteItemsSource(object sender, DependencyPropertyChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (sender == null)
                return;

            //If we're being removed, remove the callbacks
            if (e.NewValue == null)
            {
                tb.TextChanged -= onTextChanged;
                tb.PreviewKeyDown -= onKeyDown;
            }
            else
            {
                //New source.  Add the callbacks
                tb.TextChanged += onTextChanged;
                tb.PreviewKeyDown += onKeyDown;
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