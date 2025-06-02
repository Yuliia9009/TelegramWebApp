let token = "";
let currentChat = "";
let currentUserId = "";

// Отправка кода
function sendCode() {
    const countryCode = document.getElementById("countrySelect").value;
    const phone = document.getElementById("phoneInput").value;
    const fullPhone = `${countryCode}${phone.replace(/\D/g, "")}`;

    fetch("/api/auth/send-code", {
        method: "POST",
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ phoneNumber: fullPhone })
    })
    .then(async res => {
        if (!res.ok) {
            const text = await res.text();
            alert("Ошибка: " + text);
        } else {
            const data = await res.json();
            alert(data.message || "Код отправлен");
        }
    })
    .catch(err => {
        alert("Сетевая ошибка: " + err.message);
    });
}
// function sendCode() {
//     const phone = document.getElementById("phone").value;
//     fetch("/api/auth/send-code", {
//         method: "POST",
//         headers: { 'Content-Type': 'application/json' },
//         body: JSON.stringify({ phoneNumber: phone })
//     })
//         .then(async res => {
//             if (!res.ok) {
//                 const text = await res.text();
//                 alert("Ошибка: " + text);
//             } else {
//                 const data = await res.json();
//                 alert(data.message || "Код отправлен");
//             }
//         })
//         .catch(err => {
//             alert("Сетевая ошибка: " + err.message);
//         });
// }

// Верификация кода
function verifyCode() {
    const phone = document.getElementById("phone").value;
    const code = document.getElementById("otp").value;

    fetch("/api/auth/verify-code", {
        method: "POST",
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ PhoneNumber: phone, Code: code })
    })
    .then(res => {
        if (!res.ok) throw new Error("Код неправильный");
        return res.json();
    })
    .then(data => {
        console.log("Ответ от сервера:", data);
        token = data.token;

        if (!data.user.nickname || data.user.nickname.startsWith("User_")) {
            alert("Добро пожаловать! Пожалуйста, заполните свой профиль.");
            // можно автоматически показать модальное окно
        }

        document.getElementById("auth").classList.add("hidden");
        document.getElementById("main").classList.remove("hidden");
        loadContacts();
        setupSignalR();
    })
    .catch(err => alert("Ошибка: " + err.message));
}

// Выход
function logout() {
    token = "";
    location.reload();
}

// Загрузка списка пользователей
function loadContacts() {
    fetch("/api/users", {
        headers: { 'Authorization': 'Bearer ' + token }
    })
        .then(res => res.json())
        .then(users => {
            const ul = document.getElementById("contactList");
            ul.innerHTML = "";
            users.forEach(u => {
                const li = document.createElement("li");
                li.className = "chat-item";
                li.innerHTML = `
                    <img src="img/default-avatar.png" class="avatar" alt="avatar" />
                    <div class="chat-info">
                        <div class="chat-header">
                            <span class="chat-name">${u.nickname}</span>
                            <span class="chat-time">12:45</span>
                        </div>
                        <div class="chat-message">Напишите что-нибудь...</div>
                    </div>
                `;
                li.onclick = () => openChat(u.id);
                ul.appendChild(li);
            });
        });
}

// Открыть чат
function openChat(userId) {
    currentChat = userId;
    document.getElementById("messages").innerHTML = "";

    fetch(`/api/chats/${userId}/messages`, {
        headers: { 'Authorization': 'Bearer ' + token }
    })
        .then(res => res.json())
        .then(msgs => {
            msgs.forEach(m => {
                const div = document.createElement("div");
                div.className = m.senderId === currentUserId ? "message outgoing" : "message incoming";
                div.textContent = m.text;
                document.getElementById("messages").appendChild(div);
            });
        });
}

// Отправить сообщение
function sendMessage() {
    const input = document.getElementById("messageInput");
    if (!input.value.trim()) return;

    fetch(`/api/chats/${currentChat}/messages`, {
        method: "POST",
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token
        },
        body: JSON.stringify({ text: input.value })
    }).then(() => {
        input.value = "";
    });
}

// Отправить файл
function sendFile() {
    const file = document.getElementById("fileInput").files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append("file", file);

    fetch(`/api/upload/${currentChat}`, {
        method: "POST",
        headers: { 'Authorization': 'Bearer ' + token },
        body: formData
    }).then(() => alert("Файл отправлен!"));
}

// Кнопка "Добавить друга"
function addFriend() {
    const phone = prompt("Введите номер телефона друга (в формате +380...)");
    if (!phone || !phone.startsWith("+")) {
        alert("Некорректный номер.");
        return;
    }

    fetch("/api/friends/add", {
        method: "POST",
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ' + token
        },
        body: JSON.stringify({ phoneNumber: phone })
    })
    .then(res => {
        if (!res.ok) throw new Error("Пользователь не найден или уже добавлен");
        return res.json();
    })
    .then(data => {
        // после добавления друга создаём чат
        return fetch("/api/chats/private", {
            method: "POST",
            headers: {
                'Content-Type': 'application/json',
                'Authorization': 'Bearer ' + token
            },
            body: JSON.stringify({
                participantId: data.friendId 
            })
        });
    })
    .then(res => {
        if (!res.ok) throw new Error("Не удалось создать чат");
        return res.json();
    })
    .then(() => {
        alert("Друг добавлен, чат создан!");
        loadContacts(); // обновим список
    })
    .catch(err => alert("Ошибка: " + err.message));
}

    function loadContacts() {
    fetch("/api/users/friends", {
        headers: { 'Authorization': 'Bearer ' + token }
    })
    .then(res => res.json())
    .then(users => {
        const ul = document.getElementById("contactList");
        ul.innerHTML = "";
        users.forEach(u => {
            const li = document.createElement("li");
            li.className = "chat-item";
            li.innerHTML = `
              <img src="img/default-avatar.png" class="avatar" alt="avatar" />
              <div class="chat-info">
                <div class="chat-header">
                  <span class="chat-name">${u.nickname}</span>
                  <span class="chat-time">—</span>
                </div>
                <div class="chat-message">Нажмите, чтобы начать</div>
              </div>
            `;
            li.onclick = () => openChat(u.id);
            ul.appendChild(li);
        });
    });
}

// Назад из чата
function goBack() {
    document.getElementById("messages").innerHTML = "";
    currentChat = "";
}

// Подключение к SignalR
let connection;
function setupSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub", { accessTokenFactory: () => token })
        .build();

    connection.on("ReceiveMessage", message => {
        if (message.chatId === currentChat) {
            const div = document.createElement("div");
            div.className = message.senderId === currentUserId ? "message outgoing" : "message incoming";
            div.textContent = message.text;
            document.getElementById("messages").appendChild(div);
        }
    });

    connection.start()
        .then(() => console.log("SignalR подключен"))
        .catch(err => console.error(err.toString()));
}