let token = "";
let currentChat = "";

function sendCode() {
    const phone = document.getElementById("phone").value;
    fetch("/api/auth/send-code", {
        method: "POST",
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ phoneNumber: phone })
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
        token = data.token;
        document.getElementById("auth").classList.add("hidden");
        document.getElementById("main").classList.remove("hidden");
        loadContacts();
        setupSignalR();
    })
    .catch(err => alert(err.message));
}

function logout() {
    token = "";
    location.reload();
}

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
            li.textContent = u.nickname;
            li.onclick = () => openChat(u.id);
            ul.appendChild(li);
        });
    });
}

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
            div.textContent = `${m.text}`;
            document.getElementById("messages").appendChild(div);
        });
    });
}

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
    })
    .then(() => {
        input.value = "";
    });
}

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

// SignalR (автообновление сообщений)
let connection;
function setupSignalR() {
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/chatHub", { accessTokenFactory: () => token })
        .build();

    connection.on("ReceiveMessage", message => {
        if (message.chatId === currentChat) {
            const div = document.createElement("div");
            div.textContent = message.text;
            document.getElementById("messages").appendChild(div);
        }
    });

    connection.start().then(() => {
        console.log("SignalR подключен");
    }).catch(err => console.error(err.toString()));
}