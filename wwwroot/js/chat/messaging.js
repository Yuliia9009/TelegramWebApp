export function initMessaging(token, currentUserId) {
  let currentChat = "";

  window.openChat = (userId) => {
    currentChat = userId;
    document.getElementById("messages").innerHTML = "";

    fetch(`/api/chats/${userId}/messages`, {
      headers: { Authorization: "Bearer " + token }
    })
      .then(res => res.json())
      .then(msgs => {
        const container = document.getElementById("messages");
        msgs.forEach(m => {
          const div = document.createElement("div");
          div.className = m.senderId === currentUserId ? "message outgoing" : "message incoming";
          div.textContent = m.text;
          container.appendChild(div);
        });
      });
  };

  document.getElementById("sendBtn").onclick = () => {
    const input = document.getElementById("messageInput");
    if (!input.value.trim()) return;

    fetch(`/api/chats/${currentChat}/messages`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: "Bearer " + token
      },
      body: JSON.stringify({ text: input.value })
    }).then(() => {
      input.value = "";
    });
  };

  document.getElementById("sendFileBtn").onclick = () => {
    const fileInput = document.getElementById("fileInput");
    const file = fileInput.files[0];
    if (!file) return;

    const formData = new FormData();
    formData.append("file", file);

    fetch(`/api/upload/${currentChat}`, {
      method: "POST",
      headers: { Authorization: "Bearer " + token },
      body: formData
    })
      .then(() => {
        alert("Файл отправлен!");
        fileInput.value = "";
      });
  };

  document.getElementById("backBtn").onclick = () => {
    document.getElementById("messages").innerHTML = "";
    currentChat = "";
  };

  const connection = new signalR.HubConnectionBuilder()
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

  connection.on("MessageDelivered", (messageId, deliveredAt) => {
    console.log(`Сообщение доставлено: ${messageId} в ${deliveredAt}`);
  });

  connection.on("MessageRead", (messageId, readAt) => {
    console.log(`Сообщение прочитано: ${messageId} в ${readAt}`);
  });

  connection.start()
    .then(() => console.log("SignalR подключен"))
    .catch(err => console.error("Ошибка SignalR:", err));
}
