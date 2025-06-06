import { initProfile } from "./profile.js";
import { initMenu } from "./menu.js";
import { initMessaging } from "./messaging.js";
import { initContacts } from "./contacts.js";

export function initChatPage(data) {
  const token = sessionStorage.getItem("token");
  const currentUser = JSON.parse(sessionStorage.getItem("user") || "{}");

  if (!token || !currentUser.id) {
    alert("Не авторизован или пользователь не найден");
    return;
  }

  console.log("👤 Текущий пользователь:", currentUser);

  // Левая панель
  initMenu(token);
  initProfile(token);
  initContacts(token, currentUser.id);

  // ⏳ Ждём появления #messages перед initMessaging
  const waitForMessages = () => {
    const messagesContainer = document.getElementById("messages");
    if (!messagesContainer) return setTimeout(waitForMessages, 50);

    initMessaging(token, currentUser.id);

    if (data?.userId) {
      setTimeout(() => {
        if (typeof window.openChat === "function") {
          window.openChat(data.userId, true);
        } else {
          console.error("❌ window.openChat не определен");
        }
      }, 100);
    }
  };

  waitForMessages();
}
