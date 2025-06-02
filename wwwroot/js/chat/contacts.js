export function initContacts(token, currentUserId) {
  console.log("TOKEN:", token);
  fetch("/api/users/friends", {
    headers: {
      Authorization: "Bearer " + token
    }
  })
    .then(res => {
      if (!res.ok) {
        throw new Error("Ошибка при получении списка друзей: " + res.status);
      }
      return res.json();
    })
    .then(data => {
      console.log("Ответ от /api/users/friends:", data);

      const users = Array.isArray(data) ? data : data.users;
      if (!Array.isArray(users)) {
        console.error("Некорректный формат данных:", users);
        return;
      }

      const ul = document.getElementById("contactList");
      if (!ul) {
        console.error("Элемент с id contactList не найден");
        return;
      }

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
        li.onclick = () => window.openChat(u.id, token, currentUserId);
        ul.appendChild(li);
      });
    })
    .catch(err => {
      console.error("Ошибка загрузки контактов:", err);
    });
}