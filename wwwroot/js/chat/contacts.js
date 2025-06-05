export function initContacts(token, currentUserId) {
  console.log("🔑 TOKEN:", token);
  fetch("/api/friends", {
    headers: { Authorization: "Bearer " + token }
  })
    .then(res => {
      if (!res.ok) throw new Error("Ошибка при получении списка друзей: " + res.status);
      return res.json();
    })
    .then(users => {
      if (!Array.isArray(users)) {
        console.error("❌ Ожидался массив пользователей, получено:", users);
        return;
      }

      const ul = document.getElementById("contactList");
      if (!ul) {
        console.error("❌ Элемент с id 'contactList' не найден в DOM");
        return;
      }

      ul.innerHTML = "";

      users
        .filter(u => u?.id && u?.nickname)
        .sort((a, b) => a.nickname.localeCompare(b.nickname))
        .forEach(u => {
          const li = document.createElement("li");
          li.className = "chat-item";
          li.dataset.userid = u.id;

          const statusDot = u.isOnline ? "🟢" : "⚫";

          li.innerHTML = `
            <img src="img/default-avatar.png" class="avatar" alt="avatar" onerror="this.src='img/default-avatar.png'" />
            <div class="chat-info">
              <div class="chat-header">
                <span class="chat-name">${u.nickname}</span>
                <span class="chat-time">${statusDot}</span>
              </div>
              <div class="chat-message">Нажмите, чтобы начать</div>
            </div>
          `;

          li.onclick = () => {
            try {
              console.log("💬 Открывается чат с пользователем:", u.id);
              window.openChat(u.id, true);
            } catch (e) {
              console.error("Ошибка при открытии чата:", e);
            }
          };

          ul.appendChild(li);
        });
    })
    .catch(err => {
      console.error("❌ Ошибка загрузки контактов:", err);
      alert("Не удалось загрузить список друзей. Проверьте соединение.");
    });
}