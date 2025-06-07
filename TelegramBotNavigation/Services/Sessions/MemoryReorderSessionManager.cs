namespace TelegramBotNavigation.Services.Sessions
{
    public class MemoryReorderSessionManager : IReorderSessionManager
    {
        private readonly Dictionary<long, MenuReorderSession> _sessions = new();

        public Task StartAsync(long userId, MenuReorderSession session)
        {
            _sessions[userId] = session;
            return Task.CompletedTask;
        }

        public Task<MenuReorderSession?> GetAsync(long userId)
        {
            _sessions.TryGetValue(userId, out var session);
            return Task.FromResult(session);
        }

        public Task UpdateAsync(long userId, MenuReorderSession session)
        {
            if (_sessions.ContainsKey(userId))
                _sessions[userId] = session;
            return Task.CompletedTask;
        }

        public Task ClearAsync(long userId)
        {
            _sessions.Remove(userId);
            return Task.CompletedTask;
        }

        public Task MoveItemAsync(long userId, string direction)
        {
            if (!_sessions.TryGetValue(userId, out var session)) return Task.CompletedTask;

            var selectedItem = session.Items.FirstOrDefault(i => i.Id == session.SelectedItemId);
            if (selectedItem == null) return Task.CompletedTask;

            switch (direction)
            {
                case "Up": 
                    MoveUp(session, selectedItem); 
                    break;
                case "Down": 
                    MoveDown(session, selectedItem);
                    break;
                case "Left": 
                    MoveLeft(session, selectedItem);
                    break;
                case "Right": 
                    MoveRight(session, selectedItem);
                    break;
                default: return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        private void MoveUp(MenuReorderSession session, MenuItemSnapshot selectedItem)
        {
            int currentRow = selectedItem.Row;
            int currentOrder = selectedItem.Order;

            var currentRowItems = session.Items.Where(i => i.Row == currentRow).OrderBy(i => i.Order).ToList();
            if (currentRow == 0)
            {
                if (currentRowItems.Count == 1) return; // Если единственный элемент в первой строке, не перемещаем

                selectedItem.Order = 0; // Перемещаем выбранный элемент в начало текущей строки
                currentRowItems.Remove(selectedItem); // Удаляем выбранный элемент из текущей строки

                for (int i = 0; i < currentRowItems.Count; i++)
                {
                    currentRowItems[i].Order = i; // Пересчитываем порядок для всех оставшихся элементов строки
                }

                foreach (var item in session.Items)
                {
                    if (item.Id != selectedItem.Id)
                    {
                        item.Row +=1; // Увеличиваем номер строки для всех остальных элементов
                    }
                }
            }
            else
            {
                if (currentRowItems.Count == 1)
                {
                    var targetRowItems = session.Items.Where(i => i.Row == currentRow - 1).OrderBy(i => i.Order).ToList();

                    foreach (var item in targetRowItems)
                    {
                        item.Order += 1; // Увеличиваем порядок для всех остальных элементов в целевой строке
                    }

                    // Перемещаем выбранный элемент на предыдущую строку
                    selectedItem.Row = currentRow - 1;
                    selectedItem.Order = 0;

                    foreach (var item in session.Items.Where(i => i.Row > currentRow))
                    {
                        item.Row -= 1; // Уменьшаем номер строки для всех остальных элементов ниже текущей строки
                    }
                }
                else
                {
                    selectedItem.Order = 0; // Перемещаем выбранный элемент в начало текущей строки
                    currentRowItems.Remove(selectedItem); // Удаляем выбранный элемент из текущей строки

                    for (int i = 0; i < currentRowItems.Count; i++)
                    {
                        currentRowItems[i].Order = i; // Пересчитываем порядок для всех оставшихся элементов строки
                    }

                    foreach (var item in session.Items.Where(i => i.Row >= currentRow && i.Id != selectedItem.Id))
                    {
                        if (item.Id != selectedItem.Id)
                        {
                            item.Row += 1; // Увеличиваем номер строки для всех остальных элементов
                        }
                    }
                }
            }
        }

        private void MoveDown(MenuReorderSession session, MenuItemSnapshot selectedItem)
        {
            int currentRow = selectedItem.Row;
            int currentOrder = selectedItem.Order;

            int maxRow = session.Items.Max(i => i.Row);

            var currentRowItems = session.Items.Where(i => i.Row == currentRow).OrderBy(i => i.Order).ToList();

            if (currentRow == maxRow)
            {
                if (currentRowItems.Count == 1) return;

                currentRowItems.Remove(selectedItem);
                for (int i = 0; i < currentRowItems.Count; i++)
                {
                    currentRowItems[i].Order = i;
                }

                selectedItem.Row = currentRow + 1;
                selectedItem.Order = 0;
            }
            else
            {
                if (currentRowItems.Count == 1)
                {
                    selectedItem.Order = 0;

                    var targetRowItems = session.Items.Where(i => i.Row == selectedItem.Row +1).OrderBy(i => i.Order).ToList();

                    foreach (var item in targetRowItems)
                    {
                        item.Order += 1; // Увеличиваем порядок для всех остальных элементов в целевой строке
                    }

                    foreach (var item in session.Items.Where(i => i.Row > currentRow))
                    {
                        item.Row -= 1; // Уменьшаем номер строки для всех остальных элементов ниже текущей строки
                    }
                }
                else
                {
                    currentRowItems.Remove(selectedItem);

                    for (int i = 0; i < currentRowItems.Count; i++)
                    {
                        currentRowItems[i].Order = i;
                    }

                    foreach (var item in session.Items.Where(i => i.Row > currentRow))
                    {
                        if (item.Id != selectedItem.Id)
                        {
                            item.Row += 1; // Увеличиваем номер строки для всех остальных элементов
                        }
                    }

                    selectedItem.Row = currentRow + 1; // Перемещаем выбранный элемент на следующую строку
                    selectedItem.Order = 0; // Устанавливаем порядок в начало новой строки
                }
            }
        }

        private void MoveLeft(MenuReorderSession session, MenuItemSnapshot selectedItem)
        {
            if (selectedItem.Order == 0) return;

            var leftNeightbor = session.Items.FirstOrDefault(i => i.Row == selectedItem.Row && i.Order == selectedItem.Order - 1);
            if (leftNeightbor == null) return;

            leftNeightbor.Order++;
            selectedItem.Order--;
        }

        private void MoveRight(MenuReorderSession session, MenuItemSnapshot selectedItem)
        {
            var rowItems = session.Items.Where(i => i.Row == selectedItem.Row).OrderBy(i => i.Order).ToList();
            if (selectedItem.Order >= rowItems.Count - 1) return;

            var rightNeightbor = rowItems.FirstOrDefault(i => i.Order == selectedItem.Order + 1);
            if (rightNeightbor == null) return;

            rightNeightbor.Order--;
            selectedItem.Order++;
        }

        public Task MoveItemAsyncOld(long userId, string direction)
        {
            if (!_sessions.TryGetValue(userId, out var session)) return Task.CompletedTask;

            var selectedItem = session.Items.FirstOrDefault(i => i.Id == session.SelectedItemId);
            if (selectedItem == null) return Task.CompletedTask;

            int dx = 0, dy = 0;
            switch (direction)
            {
                case "Up": dy = -1; break;
                case "Down": dy = 1; break;
                case "Left": dx = -1; break;
                case "Right": dx = 1; break;
                default: return Task.CompletedTask;
            }

            var currentRow = selectedItem.Row;
            var currentOrder = selectedItem.Order;

            var currentRowItems = session.Items.Where(i => i.Row == currentRow).OrderBy(i => i.Order).ToList();

            int maxRow = session.Items.Max(i => i.Row);
            bool isInLastRow = currentRow == maxRow;
            bool isAloneInRow = currentRowItems.Count == 1;
            bool wantsToGoDown = dy > 0;

            if (isInLastRow && isAloneInRow && wantsToGoDown)
            {
                // If alone on last row, can't move further down
                return Task.CompletedTask;
            }

            int newRow = Math.Max(0, currentRow + dy);

            var targetRowItems = session.Items.Where(i => i.Row == newRow).OrderBy(i => i.Order).ToList();

            // Clamp order inside target row boundaries
            int newOrder = Math.Clamp(currentOrder + dx, 0, targetRowItems.Count);

            if (currentRow == newRow)
            {
                // Moving inside the same row (left/right)
                currentRowItems.Remove(selectedItem);
                currentRowItems.Insert(newOrder, selectedItem);

                // Reassign orders
                for (int i = 0; i < currentRowItems.Count; i++)
                {
                    currentRowItems[i].Order = i;
                }

                // Update session
                session.Items = session.Items
                    .Where(i => i.Row != currentRow)
                    .Concat(currentRowItems)
                    .OrderBy(i => i.Row)
                    .ThenBy(i => i.Order)
                    .ToList();
            }
            else
            {
                // Moving across rows (up/down)
                currentRowItems.Remove(selectedItem);
                for (int i = 0; i < currentRowItems.Count; i++)
                {
                    currentRowItems[i].Order = i;
                }

                targetRowItems.Insert(newOrder, selectedItem);
                for (int i = 0; i < targetRowItems.Count; i++)
                {
                    targetRowItems[i].Order = i;
                }

                // Update selectedItem
                selectedItem.Row = newRow;
                selectedItem.Order = newOrder;

                // Update session
                session.Items = session.Items
                    .Where(i => i.Row != currentRow && i.Row != newRow)
                    .Concat(currentRowItems)
                    .Concat(targetRowItems)
                    .OrderBy(i => i.Row)
                    .ThenBy(i => i.Order)
                    .ToList();
            }

            session.SelectedItemId = selectedItem.Id;

            return Task.CompletedTask;
        }

    }
}
