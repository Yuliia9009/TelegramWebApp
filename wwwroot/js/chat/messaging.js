export function initMessaging(token, currentUserId) {
  if (window.__messagingInitialized) return;
  window.__messagingInitialized = true;

  let currentChat = "";

  const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5032/chatHub", { accessTokenFactory: () => token })
    .build();

  function scrollToBottom() {
    const container = document.getElementById("messages");
    if (container) container.scrollTop = container.scrollHeight;
  }

  function renderMessage(message) {
    const container = document.getElementById("messages");
    if (!container) return;

    const div = document.createElement("div");
    div.id = `msg-${message.id}`;
    div.className = message.senderId === currentUserId ? "message outgoing" : "message incoming";

    let status = "";
    if (message.senderId === currentUserId) {
      if (message.readAt) status = "👁";
      else if (message.deliveredAt) status = "✅";
      else status = "⌛";
    }

    const content = message.text || message.fileUrl
      ? `<a href="${message.fileUrl}" target="_blank">${message.text || "📎 Файл"}</a>`
      : "";

    div.innerHTML = `${content} <span class="status" id="status-${message.id}">${status}</span>`;
    container.appendChild(div);
    scrollToBottom();
  }

  connection.on("ReceiveMessage", message => {
    if (message.chatId === currentChat) {
      renderMessage(message);
    }
  });

  connection.on("MessageDelivered", messageId => {
    const el = document.getElementById(`status-${messageId}`);
    if (el) el.textContent = "✅";
  });

  connection.on("MessageRead", messageId => {
    const el = document.getElementById(`status-${messageId}`);
    if (el) el.textContent = "👁";
  });

  connection.on("UserCameOnline", userId => {
    const el = document.getElementById("user-status");
    const panel = document.querySelector("main.chat-panel");
    if (panel?.dataset.chatUserId === userId && el) el.textContent = "🟢 онлайн";
  });

  connection.on("UserWentOffline", userId => {
    const el = document.getElementById("user-status");
    const panel = document.querySelector("main.chat-panel");
    if (panel?.dataset.chatUserId === userId && el) el.textContent = "⚫ оффлайн";
  });

  connection.start().then(() => {
    console.log("✅ SignalR подключен");

    window.openChat = (userId, isFromContacts = true) => {
      const tryOpen = () => {
        const container = document.getElementById("messages");
        if (!container) return setTimeout(tryOpen, 50);

        const url = isFromContacts
          ? `/api/chats/find-or-create/${userId}`
          : `/api/chats/${userId}`;

        fetch(url, {
          headers: { Authorization: "Bearer " + token }
        })
          .then(res => res.json())
          .then(chat => {
            currentChat = chat.id;
            container.innerHTML = "";

            const panel = document.querySelector("main.chat-panel");
            if (panel) panel.dataset.chatUserId = chat.otherUserId || "";

            const otherUserId = chat.otherUserId;
            const isValidGuid = /^[0-9a-fA-F-]{36}$/.test(otherUserId);

            if (isValidGuid) {
              fetch(`/api/users/${otherUserId}/status`, {
                headers: { Authorization: "Bearer " + token }
              })
                .then(res => {
                  if (!res.ok) throw new Error(`Статус ${res.status}`);
                  return res.json();
                })
                .then(status => {
                  const el = document.getElementById("user-status");
                  if (el) el.textContent = status.isOnline ? "🟢 онлайн" : "⚫ оффлайн";
                })
                .catch(err => console.warn("⚠️ Ошибка получения статуса:", err));
            } else {
              console.warn("⚠️ Некорректный или отсутствует chat.otherUserId:", otherUserId);
            }

            connection.invoke("JoinChat", currentChat);

            fetch(`/api/chats/${currentChat}/messages`, {
              headers: { Authorization: "Bearer " + token }
            })
              .then(res => res.json())
              .then(messages => {
                container.innerHTML = "";
                messages.forEach(renderMessage);
              });
          })
          .catch(err => {
            console.error("❌ Ошибка открытия чата:", err);
            alert("Не удалось открыть чат: " + err.message);
          });
      };

      tryOpen();
    };
  });

  document.addEventListener("click", e => {
    if (e.target?.id === "sendBtn") {
      const input = document.getElementById("messageInput");
      const text = input?.value.trim();
      if (!text || !currentChat) return;

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
          if (!res.ok) throw new Error("Ошибка отправки");
          input.value = "";
        })
        .catch(err => {
          input.disabled = false;
          alert("Ошибка: " + err.message);
        });
    }

    if (e.target?.id === "sendFileBtn") {
      const fileInput = document.getElementById("fileInput");
      const file = fileInput?.files?.[0];
      if (!file || !currentChat) return;

      const formData = new FormData();
      formData.append("file", file);

      fetch(`/api/upload/${currentChat}`, {
        method: "POST",
        headers: { Authorization: "Bearer " + token },
        body: formData
      })
        .then(res => {
          if (!res.ok) throw new Error("Ошибка загрузки файла");
          alert("📎 Файл отправлен");
          fileInput.value = "";
        })
        .catch(err => alert("Ошибка отправки файла: " + err.message));
    }

    if (e.target?.id === "backBtn") {
      const container = document.getElementById("messages");
      if (container) container.innerHTML = "";
      currentChat = "";
    }
  });
}