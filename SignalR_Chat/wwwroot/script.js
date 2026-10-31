document.addEventListener('DOMContentLoaded', () => {
    const loginBlock = document.getElementById('loginBlock');
    const chatBody = document.getElementById('chatBody');
    const btnLogin = document.getElementById('btnLogin');
    const txtUserName = document.getElementById('txtUserName');
    const btnSend = document.getElementById('sendmessage');
    const txtMessage = document.getElementById('message');
    const chatroom = document.getElementById('chatroom');
    const chatusers = document.getElementById('chatusers');
    const header = document.getElementById('header');
    const hdId = document.getElementById('hdId');
    const hdUsername = document.getElementById('username');

    /*
    Для взаємодії з хабом ChatHub за допомогою методу build() об'єкта HubConnectionBuilder
    створюється об'єкт hubConnection - об'єкт підключення.
    Метод withUrl встановлює адресу, за якою програма буде звертатися до хабу.
    Оскільки ChatHub на сервері зіставляється з адресою "/chat", то саме ця адреса і передається у withUrl.
    */
    const hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("/chat")
        .build();

    /*
    Якщо адреса сервера та адреса клієнта не збігатимуться, то знадобиться налаштувати підтримку CORS.
    У цьому випадку серверна та клієнтська частини працюють у рамках одного застосунку, тому налаштування CORS не потрібне.
    */

    // Метод hubConnection.on встановлює функцію на боці клієнта,
    // яка отримуватиме дані від сервера (хаба)
    hubConnection.on("AddMessage", (name, message) => {
        // Додавання повідомлень на веб-сторінку
        const msgElement = document.createElement('div');
        msgElement.className = 'message';
        msgElement.innerHTML = `<b>${htmlEncode(name)}</b>: <i>${htmlEncode(message)}</i>`;
        chatroom.appendChild(msgElement);
        chatroom.scrollTop = chatroom.scrollHeight;
    });

    // Функція, що викликається сервером під час підключення нового користувача
    hubConnection.on("Connected", (id, userName, allUsers) => {
        loginBlock.style.display = 'none';
        chatBody.style.display = 'block';

        // Встановлення у прихованих полях імені та id поточного користувача
        hdId.value = id;
        hdUsername.value = userName;
        header.innerHTML = `<h3>Ласкаво просимо, ${htmlEncode(userName)}!</h3>`;

        // Додавання всіх користувачів
        chatusers.innerHTML = '';
        allUsers.forEach(user => AddUser(user.connectionId, user.login));
    });

    // Функція, що викликається сервером для додавання нового користувача
    hubConnection.on("NewUserConnected", (id, name) => {
        AddUser(id, name);
    });

    // Функція, що викликається сервером для видалення користувача
    hubConnection.on("UserDisconnected", (id, userName) => {
        const userElement = document.getElementById(id);
        if (userElement) userElement.remove();
    });

    // Відкриваємо з'єднання із сервером.
    // Якщо підключення до хабу успішно встановлено,
    // то спрацює метод then, щоб виконати певні дії.
    // Якщо ж у процесі підключення до сервера виникне помилка,
    // то спрацює функція, яка передається в метод catch і яка отримає дані про помилку.
    hubConnection.start()
        .then(() => {
            // відправлення повідомлення
            btnSend.addEventListener('click', () => {
                const msg = txtMessage.value.trim();
                if (!msg) return;

                // Викликаємо у хаба метод Send і передаємо йому дані.
                // У разі, якщо під час відправлення виникне помилка,
                // то спрацює функція, яка передається в метод catch().
                hubConnection.invoke("Send", hdUsername.value, msg)
                    .catch(err => console.error(err.toString()));
                txtMessage.value = '';
            });

            txtMessage.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') btnSend.click();
            });

            // обробка логіна
            btnLogin.addEventListener('click', () => {
                const name = txtUserName.value.trim();
                if (name.length > 0) {
                    // Викликаємо у хаба метод Connect і передаємо йому дані.
                    // У разі, якщо під час відправлення виникне помилка,
                    // то спрацює функція, яка передається в метод catch().
                    hubConnection.invoke("Connect", name)
                        .catch(err => console.error(err.toString()));
                } else {
                    alert("Будь ласка, введіть ім'я");
                }
            });

            txtUserName.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') btnLogin.click();
            });
        })
        .catch(err => console.error(err.toString()));

    // Кодування тегів
    function htmlEncode(value) {
        const div = document.createElement('div');
        div.textContent = value;
        return div.innerHTML;
    }

    // Додавання нового користувача
    function AddUser(id, name) {
        const currentId = hdId.value;
        if (name && currentId !== id) {
            const li = document.createElement('li');
            li.id = id;
            li.innerHTML = `<b>${htmlEncode(name)}</b>`;
            chatusers.appendChild(li);
        }
    }
});