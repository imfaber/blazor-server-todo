using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazorTodo.Shared;
using Cloudcrate.AspNetCore.Blazor.Browser.Storage;
using Newtonsoft.Json;

namespace BlazorTodo.Client.Services {
    public class AppState {
        private static string storageKey = "todos";
        public string PageTitle { get; private set; }
        private LocalStorage Storage { get; set; }
        public List<TodoItem> TodoList { get; private set; } = new List<TodoItem>();

        // Lets components receive change notifications
        public event Action OnChange;

        public async Task Init(LocalStorage storage) {
            Storage = storage;
            var jsonString = await Storage.GetItemAsync(storageKey);
            if (jsonString != null) {
                TodoList = JsonConvert.DeserializeObject<List<TodoItem>>(jsonString);
                NotifyStateChanged();
            }
        }

        public void SetPageTitle(string title) {
            PageTitle = title;
            NotifyStateChanged();
        }

        public List<TodoItem> GetTodoList(FilterCriteria filter) {
            switch (filter) {
                case FilterCriteria.Completed:
                    return TodoList.FindAll(x => x.IsDone);
                case FilterCriteria.Active:
                    return TodoList.FindAll(x => !x.IsDone);
                default:
                    return TodoList;
            }
        }

        private async Task PersistTodoList() {
            await Storage.SetItemAsync(storageKey, JsonConvert.SerializeObject(TodoList));
            NotifyStateChanged();
        }

        public async Task AddTodo(string todo) {
            TodoList.Add(new TodoItem {
                Title = todo,
                IsDone = false
            });
            await PersistTodoList();
        }

        public async Task RemoveTodo(TodoItem todoItem) {
            TodoList.Remove(todoItem);
            await PersistTodoList();
        }

        public async Task ToggleTodo(TodoItem todoItem) {
            todoItem.IsDone = !todoItem.IsDone;
            await PersistTodoList();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}