using System;
using System.Collections.Generic;
using System.Text;
using GateWay.Views;


namespace CoreClasses
{
    public class Templates
    {
        private readonly KeyStorage _keyStorage;
        private readonly ChatStorage _chatStorage;

        public Templates(string rootPath)
        {
            _keyStorage = new KeyStorage(rootPath);
            _chatStorage = new ChatStorage(rootPath);
        }

        public bool IsUserRegistered()
        {
            return _keyStorage.KeysExist();
        }

        public void LoadAllChats()
        {
            foreach (ChatPreview Chat in _chatStorage.GetAllChats()) {
                MainWindow.Context.AddChatToList(
                    Chat.ChatId,
                    Chat.Name,
                    Chat.LastMessage,
                    Chat.LastSenderId != null
                    );
            }
        }

        // TODO: Регистрация пользователя
        // TODO: передать функцию для передачи сообщения И чата тебе в бэк =
        // TODO: фолдер для чата
        // TODO: для отрисовки сообщения ответ
        // TODO: удаление чата(папки по ID)
    }
}
