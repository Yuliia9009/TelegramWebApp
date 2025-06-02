import { initProfile } from "./profile.js";
import { initMenu } from "./menu.js";
import { initMessaging } from "./messaging.js";
import { initContacts } from "./contacts.js";

export function initChatPage() {
  const token = sessionStorage.getItem("token");
  const currentUser = JSON.parse(sessionStorage.getItem("user") || "{}");

  if (!token) {
    alert("Не авторизован");
    return;
  }

  initMenu(token);
  initProfile(token);
  initContacts(token, currentUser.id);
  initMessaging(token, currentUser.id);
}

window.initChatPage = initChatPage;

// export function initChatPage() {
//   const token = sessionStorage.getItem("token");
//   const currentUser = JSON.parse(sessionStorage.getItem("user") || "{}");
//   const currentUserId = currentUser.id;
//   let currentChat = "";

//   if (!token) {
//     alert("Не авторизован");
//     return;
//   }

//   // Навешиваем обработчики на UI
//   document.getElementById("logout").onclick = logout;
//   document.getElementById("sendBtn").onclick = sendMessage;
//   document.getElementById("sendFileBtn").onclick = sendFile;
//   document.getElementById("backBtn").onclick = goBack;
//   document.getElementById("addFriend").onclick = addFriend;

//   // Выпадающее меню по нажатию на аватар
//   const avatar = document.getElementById("menuAvatar");
//   const dropdownMenu = document.getElementById("dropdownMenu");

//   avatar?.addEventListener("click", (e) => {
//     e.stopPropagation();
//     dropdownMenu?.classList.toggle("show");
//   });

//   document.addEventListener("click", (e) => {
//     if (!dropdownMenu?.contains(e.target)) {
//       dropdownMenu?.classList.remove("show");
//     }
//   });

//   loadContacts();
//   setupSignalR();

//   function loadContacts() {
//     fetch("/api/users/friends", {
//       headers: { Authorization: "Bearer " + token }
//     })
//       .then(res => res.json())
//       .then(users => {
//         const list = document.getElementById("contactList");
//         list.innerHTML = "";
//         users.forEach(u => {
//           const li = document.createElement("li");
//           li.className = "chat-item";
//           li.innerHTML = `
//             <img src="img/default-avatar.png" class="avatar" alt="avatar" />
//             <div class="chat-info">
//               <div class="chat-header">
//                 <span class="chat-name">${u.nickname}</span>
//                 <span class="chat-time">—</span>
//               </div>
//               <div class="chat-message">Нажмите, чтобы начать</div>
//             </div>
//           `;
//           li.onclick = () => openChat(u.id);
//           list.appendChild(li);
//         });
//       });
//   }

//   function openChat(userId) {
//     currentChat = userId;
//     document.getElementById("messages").innerHTML = "";

//     fetch(`/api/chats/${userId}/messages`, {
//       headers: { Authorization: "Bearer " + token }
//     })
//       .then(res => res.json())
//       .then(msgs => {
//         const container = document.getElementById("messages");
//         msgs.forEach(m => {
//           const div = document.createElement("div");
//           div.className = m.senderId === currentUserId ? "message outgoing" : "message incoming";
//           div.textContent = m.text;
//           container.appendChild(div);
//         });
//       });
//   }

//   function sendMessage() {
//     const input = document.getElementById("messageInput");
//     if (!input.value.trim()) return;

//     fetch(`/api/chats/${currentChat}/messages`, {
//       method: "POST",
//       headers: {
//         "Content-Type": "application/json",
//         Authorization: "Bearer " + token
//       },
//       body: JSON.stringify({ text: input.value })
//     }).then(() => {
//       input.value = "";
//     });
//   }

//   function sendFile() {
//     const file = document.getElementById("fileInput").files[0];
//     if (!file) return;

//     const formData = new FormData();
//     formData.append("file", file);

//     fetch(`/api/upload/${currentChat}`, {
//       method: "POST",
//       headers: { Authorization: "Bearer " + token },
//       body: formData
//     }).then(() => alert("Файл отправлен!"));
//   }

//   function addFriend() {
//     const phone = prompt("Введите номер телефона друга (в формате +380...)");
//     if (!phone || !phone.startsWith("+")) {
//       alert("Некорректный номер.");
//       return;
//     }

//     fetch("/api/friends/add", {
//       method: "POST",
//       headers: {
//         "Content-Type": "application/json",
//         Authorization: "Bearer " + token
//       },
//       body: JSON.stringify({ phoneNumber: phone })
//     })
//       .then(res => {
//         if (!res.ok) throw new Error("Пользователь не найден или уже добавлен");
//         return res.json();
//       })
//       .then(data => {
//         return fetch("/api/chats/private", {
//           method: "POST",
//           headers: {
//             "Content-Type": "application/json",
//             Authorization: "Bearer " + token
//           },
//           body: JSON.stringify({ participantId: data.friendId })
//         });
//       })
//       .then(() => {
//         alert("Друг добавлен, чат создан!");
//         loadContacts();
//       })
//       .catch(err => alert("Ошибка: " + err.message));
//   }

//   function goBack() {
//     document.getElementById("messages").innerHTML = "";
//     currentChat = "";
//   }

//   function logout() {
//     sessionStorage.clear();
//     location.reload();
//   }

//   function setupSignalR() {
//     const connection = new signalR.HubConnectionBuilder()
//       .withUrl("/chatHub", { accessTokenFactory: () => token })
//       .build();

//     connection.on("ReceiveMessage", message => {
//       if (message.chatId === currentChat) {
//         const div = document.createElement("div");
//         div.className = message.senderId === currentUserId ? "message outgoing" : "message incoming";
//         div.textContent = message.text;
//         document.getElementById("messages").appendChild(div);
//       }
//     });

//     connection.start()
//       .then(() => console.log("SignalR подключен"))
//       .catch(err => console.error("Ошибка SignalR:", err));
//   }

//     document.getElementById("editProfile").onclick = openProfileModal;
//     document.getElementById("closeProfileModal").onclick = () =>
//     document.getElementById("profileModal").classList.remove("show");

//   document.getElementById("saveProfileBtn").onclick = () => {
//     const nickname = document.getElementById("nicknameInput").value;
//     const birthdate = document.getElementById("birthdateInput").value;
//     const user = JSON.parse(sessionStorage.getItem("user") || "{}");


//     fetch(`/api/users/${user.id}`, {
//       method: "PUT",
//       headers: {
//         "Content-Type": "application/json",
//         Authorization: "Bearer " + token
//       },
//       body: JSON.stringify({
//         nickname,
//         dateOfBirth: birthdate
//       })
//     })
//     .then(res => {
//       if (!res.ok) throw new Error("Ошибка обновления");
//       return res.json();
//     })
//     .then(updated => {
//       alert("Профиль обновлён");
//       sessionStorage.setItem("user", JSON.stringify(updated));
//       document.getElementById("profileModal").classList.remove("show");
//     })
//     .catch(err => alert("Ошибка: " + err.message));
//   };

//   function openProfileModal() {
//     const user = JSON.parse(sessionStorage.getItem("user") || "{}");
//     document.getElementById("nicknameInput").value = user.nickname || "";
//     document.getElementById("birthdateInput").value = user.birthdate || "";
//     document.getElementById("phoneInputEdit").value = user.phoneNumber || "";
//     document.getElementById("profileModal").classList.add("show");
//     // Установка аватарки по умолчанию (или из sessionStorage в будущем)
//     document.getElementById("avatarPreview").src = "img/default-avatar.png";

//     document.getElementById("profileModal").classList.add("show");
//   }
// }

// window.initChatPage = initChatPage;