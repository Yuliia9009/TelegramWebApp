export function initMessaging(token, currentUserId) {
  let currentChat = "";

  const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5032/chatHub", { accessTokenFactory: () => token })
    .build();

  function scrollToBottom() {
    const container = document.getElementById("messages");
    if (container) container.scrollTop = container.scrollHeight;
  }

  connection.on("ReceiveMessage", message => {
    if (message.chatId === currentChat) {
      const div = document.createElement("div");
      div.id = `msg-${message.id}`;
      div.className = message.senderId === currentUserId ? "message outgoing" : "message incoming";

      const status = message.senderId === currentUserId
        ? `<span class="status" id="status-${message.id}">⌛</span>`
        : "";

      div.innerHTML = `${message.text} ${status}`;
      document.getElementById("messages").appendChild(div);
      scrollToBottom();
    }
  });

  connection.on("MessageDelivered", (messageId, deliveredAt) => {
    const el = document.getElementById(`status-${messageId}`);
    if (el) el.textContent = "✅";
  });

  connection.on("MessageRead", (messageId, readAt) => {
    const el = document.getElementById(`status-${messageId}`);
    if (el) el.textContent = "👁";
  });

  connection.on("UserCameOnline", userId => {
  const chatUserId = document.querySelector("main.chat-panel")?.dataset.chatUserId;
  if (userId === chatUserId) {
    const el = document.getElementById("user-status");
    if (el) el.textContent = "🟢 онлайн";
  }
});

connection.on("UserWentOffline", userId => {
  const chatUserId = document.querySelector("main.chat-panel")?.dataset.chatUserId;
  if (userId === chatUserId) {
    const el = document.getElementById("user-status");
    if (el) el.textContent = "⚫ оффлайн";
  }
});

  connection.start().then(() => {
    console.log("✅ SignalR подключен");

    window.openChat = (userId, isFromContacts = true) => {
      const url = isFromContacts
        ? `/api/chats/find-or-create/${userId}`
        : `/api/chats/${userId}`;

      fetch(url, {
        headers: { Authorization: "Bearer " + token }
      })
        .then(res => {
          if (!res.ok) throw new Error(`Ошибка получения чата: ${res.status}`);
          return res.json();
        })
        .then(chat => {
          currentChat = chat.id;
          console.log("📌 Открыт чат:", currentChat);
          document.getElementById("messages").innerHTML = "";

          // 👤 Установим ID собеседника и статус
          document.querySelector("main.chat-panel").dataset.chatUserId = chat.otherUserId || ""; // предположим, что API отдаёт otherUserId

          fetch(`/api/users/${chat.otherUserId}/status`, {
            headers: { Authorization: "Bearer " + token }
          })
            .then(res => res.json())
            .then(status => {
              const el = document.getElementById("user-status");
              if (el) el.textContent = status.isOnline ? "🟢 онлайн" : "⚫ оффлайн";
            })
            .catch(err => {
              console.warn("⚠️ Не удалось получить статус пользователя:", err);
            });

          connection.invoke("JoinChat", currentChat)
            .then(() => console.log("🔗 Присоединились к чату:", currentChat))
            .catch(err => console.error("❌ Ошибка JoinChat:", err));

          fetch(`/api/chats/${currentChat}/messages`, {
            headers: { Authorization: "Bearer " + token }
          })
            .then(res => {
              if (!res.ok) throw new Error(`Ошибка загрузки сообщений: ${res.status}`);
              return res.json();
            })
            .then(msgs => {
            const container = document.getElementById("messages");
            container.innerHTML = "";
            msgs.forEach(m => {
              const div = document.createElement("div");
              div.id = `msg-${m.id}`;
              div.className = m.senderId === currentUserId ? "message outgoing" : "message incoming";

              let status = "";
              if (m.senderId === currentUserId) {
                if (m.readAt) status = "👁";
                else if (m.deliveredAt) status = "✅";
                else status = "⌛";
              }

              div.innerHTML = `${m.text} <span class="status" id="status-${m.id}">${status}</span>`;
              container.appendChild(div);
            });
            scrollToBottom();
          })
            .catch(err => {
              console.error("❌ Ошибка загрузки сообщений:", err);
              alert("Ошибка при загрузке сообщений: " + err.message);
            });
        })
        .catch(err => {
          console.error("❌ Ошибка получения чата:", err);
          alert("Не удалось загрузить чат: " + err.message);
        });
    };
  }).catch(err => {
    console.error("❌ Ошибка подключения SignalR:", err);
    alert("Ошибка подключения к серверу: " + err.message);
  });

  document.getElementById("sendBtn").onclick = () => {
    const input = document.getElementById("messageInput");
    const text = input.value.trim();
    if (!text || !currentChat) {
      console.warn("⚠️ Пустое сообщение или не выбран чат");
      return;
    }

    input.disabled = true;

    fetch(`/api/chats/${currentChat}/messages`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: "Bearer " + token
      },
      body: JSON.stringify({ text })
    })
      .then(res => {
        input.disabled = false;
        if (!res.ok) {
          return res.text().then(t => { throw new Error(`Ошибка: ${res.status} - ${t}`); });
        }
        input.value = "";
      })
      .catch(err => {
        input.disabled = false;
        console.error("❌ Ошибка при отправке сообщения:", err);
        alert("Ошибка при отправке: " + err.message);
      });
  };

  document.getElementById("sendFileBtn").onclick = () => {
    const fileInput = document.getElementById("fileInput");
    const file = fileInput.files[0];
    if (!file || !currentChat) return;

    const formData = new FormData();
    formData.append("file", file);

    fetch(`/api/upload/${currentChat}`, {
      method: "POST",
      headers: { Authorization: "Bearer " + token },
      body: formData
    })
      .then(res => {
        if (!res.ok) {
          return res.text().then(t => { throw new Error(`Ошибка: ${res.status} - ${t}`); });
        }
        alert("📎 Файл отправлен!");
        fileInput.value = "";
      })
      .catch(err => {
        console.error("❌ Ошибка при отправке файла:", err);
        alert("Ошибка при отправке файла: " + err.message);
      });
  };

  document.getElementById("backBtn").onclick = () => {
    document.getElementById("messages").innerHTML = "";
    currentChat = "";
  };
}