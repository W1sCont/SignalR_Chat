document.addEventListener('DOMContentLoaded', () => {
    const loginBlock = document.getElementById('loginBlock');
    const chatBody = document.getElementById('chatBody');
    const btnLogin = document.getElementById('btnLogin');
    const txtUserName = document.getElementById('txtUserName');
    const btnSend = document.getElementById('sendmessage');
    const txtMessage = document.getElementById('message');
    const chatroom = document.getElementById('chatroom');
    const chatusers = document.getElementById('chatusers');
    const headerTitle = document.getElementById('headerTitle');
    const emptyState = document.getElementById('emptyState');
    const hdId = document.getElementById('hdId');
    const hdUsername = document.getElementById('username');

    const hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("/chat")
        .build();

    hubConnection.on("AddMessage", (name, message) => {
        AppendMessage(name, message);
        scrollToBottomIfNear();
    });

    hubConnection.on("Connected", (id, userName, allUsers, lastMessages) => {
        loginBlock.style.display = 'none';
        chatBody.style.display = 'grid';

        hdId.value = id;
        hdUsername.value = userName;
        headerTitle.innerHTML = `Вітаємо, <b>${htmlEncode(userName)}</b>!`;

        chatusers.innerHTML = '';
        allUsers.forEach(user => AddUser(user.connectionId, user.login));

        if (lastMessages && lastMessages.length > 0) {
            if (emptyState) emptyState.remove();
            lastMessages.forEach(msg => {
                AppendMessage(msg.username, msg.mesaageText, msg.time);
            });
            chatroom.scrollTop = chatroom.scrollHeight;
        }
    });

    hubConnection.on("NewUserConnected", (id, name) => {
        AddUser(id, name);
    });

    hubConnection.on("UserDisconnected", (id, userName) => {
        const userElement = document.getElementById(id);
        if (userElement) userElement.remove();
    });

    hubConnection.start()
        .then(() => {
            btnSend.addEventListener('click', sendMessage);

            txtMessage.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') sendMessage();
            });

            btnLogin.addEventListener('click', login);

            txtUserName.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') login();
            });
        })
        .catch(err => console.error(err.toString()));

    function sendMessage() {
        const msg = txtMessage.value.trim();
        if (!msg) return;

        hubConnection.invoke("Send", hdUsername.value, msg)
            .catch(err => console.error(err.toString()));
        txtMessage.value = '';
        txtMessage.focus();
    }

    function login() {
        const name = txtUserName.value.trim();
        if (name.length > 0) {
            hubConnection.invoke("Connect", name)
                .catch(err => console.error(err.toString()));
        } else {
            alert("Будь ласка, введіть ім'я");
        }
    }

    function htmlEncode(value) {
        const div = document.createElement('div');
        div.textContent = value;
        return div.innerHTML;
    }

    function getInitials(name) {
        return name ? name.substring(0, 2).toUpperCase() : '??';
    }

    function AddUser(id, name) {
        const currentId = hdId.value;
        if (name && currentId !== id) {
            const li = document.createElement('li');
            li.id = id;
            li.innerHTML = `
                <div class="user-avatar">${getInitials(name)}</div>
                <div class="user-info">
                    <span class="user-name">${htmlEncode(name)}</span>
                </div>
                <span class="user-status" style="margin-left: auto;"></span>
            `;
            chatusers.appendChild(li);
        }
    }

    function AppendMessage(name, message, time) {
        if (document.getElementById('emptyState')) {
            document.getElementById('emptyState').remove();
        }

        const isMyMessage = name === hdUsername.value;
        const msgElement = document.createElement('div');
        const timeStr = time ? new Date(time).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        msgElement.className = `message ${isMyMessage ? 'my-message' : 'other-message'}`;

        msgElement.innerHTML = `
            ${!isMyMessage ? `<div class="message-sender">${htmlEncode(name)}</div>` : ''}
            <div class="message-bubble">
                ${htmlEncode(message)}
                <span class="message-time">${timeStr}</span>
            </div>
        `;
        chatroom.appendChild(msgElement);
    }

    function scrollToBottomIfNear() {
        const threshold = 150;
        const isNearBottom = chatroom.scrollHeight - chatroom.scrollTop - chatroom.clientHeight < threshold;
        if (isNearBottom) {
            chatroom.scrollTop = chatroom.scrollHeight;
        }
    }
});