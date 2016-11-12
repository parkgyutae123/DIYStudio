using SF_DIY_Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SF_DIY_Domain
{
    public sealed class InteriorItem
    {
        public InteriorItem(string name)
        {
            Name = name;
        }
        public string Name { get; }
    }

    public sealed class InteriorItemCategory
    {
        public InteriorItemCategory(string name, params InteriorItem[] interiorItems)
        {
            Name = name;
            InteriorItems = new ObservableCollection<InteriorItem>(interiorItems);
        }
        public string Name { get; }
        public ObservableCollection<InteriorItem> InteriorItems { get; }
    }


    public class InteriorItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private object _selectedItem;
        public object SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                this.MutateVerbose(ref _selectedItem, value, args => PropertyChanged?.Invoke(this, args));
            }
        }

        public ObservableCollection<InteriorItemCategory> InteriorItemCategories { get; }

        public AnotherCommandImplementation AddCommand { get; }

        public AnotherCommandImplementation RemoveSelectedItemCommand { get; }

        public InteriorItemViewModel()
        {
            InteriorItemCategories = new ObservableCollection<InteriorItemCategory>
            {
                new InteriorItemCategory("침대",
                new InteriorItem("싸구려"), new InteriorItem("에이스"),
                new InteriorItem("물침대"),new InteriorItem("라텍스소재")),
                new InteriorItemCategory("화장실",
                new InteriorItem("변기"),new InteriorItem("욕조"),new InteriorItem("세면대"))
            };
            AddCommand = new AnotherCommandImplementation(
                _ =>
                {
                    if (!InteriorItemCategories.Any())
                    {
                        InteriorItemCategories.Add(new InteriorItemCategory(GenerateString(15)));
                    }
                    else
                    {
                        var index = new Random().Next(0, InteriorItemCategories.Count);

                        InteriorItemCategories[index].InteriorItems.Add(new InteriorItem(GenerateString(15)));
                    }
                }
                );

            RemoveSelectedItemCommand = new AnotherCommandImplementation(
                _ =>
                {
                    var interiorCategory = SelectedItem as InteriorItemCategory;
                    if (interiorCategory != null)
                    {
                        InteriorItemCategories.Remove(interiorCategory);
                    }
                    else
                    {
                        var interiorItem = SelectedItem as InteriorItem;
                        if (interiorItem == null) return;
                        InteriorItemCategories.FirstOrDefault(v => v.InteriorItems.Contains(interiorItem))?.InteriorItems.Remove(interiorItem);
                    }
                },
                _ => SelectedItem != null
                );



        }

    
        
        private static string GenerateString(int length)
        {
            var random = new Random();

            return string.Join(string.Empty,
                Enumerable.Range(0, length)
                .Select(v => (char)random.Next('a', 'z' + 1)));
        }

    }


    public class AnotherCommandImplementation : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        public AnotherCommandImplementation(Action<object> execute) : this(execute, null)
        {
        }

        public AnotherCommandImplementation(Action<object> execute, Func<object, bool> canExecute)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));

            _execute = execute;
            _canExecute = canExecute ?? (x => true);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public void Refresh()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}
