namespace Anabranch.Neo4JConsolePackage
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Input;

    public partial class Neo4jConsoleControl : UserControl
    {
        public Neo4jConsoleControl()
        {
            InitializeComponent();
            DataContext = new Neo4jConsoleControlViewModel();
        }

        private Neo4jConsoleControlViewModel Vm { get { return (Neo4jConsoleControlViewModel) DataContext; } }

        private void CypherKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                e.Handled = true;
                Action callback = () => _results.ScrollToEnd();
                Vm.PostCommand.Execute(callback);
            }
        }

        private void CypherKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                e.Handled = true;
                if (e.Key == Key.Up)
                    Vm.NextHistoryCommand.Execute(null);

                if (e.Key == Key.Down)
                    Vm.PreviousHistoryCommand.Execute(null);

                return;
            }

            Vm.CypherQuery = ((TextBox) sender).Text;
        }
    }
}