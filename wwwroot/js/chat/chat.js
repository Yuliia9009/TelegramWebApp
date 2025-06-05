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

  if (!currentUser.id) {
    alert("Ошибка: ID пользователя не найден");
    return;
  }

  console.log("👤 Текущий пользователь:", currentUser);

  initMenu(token);
  initProfile(token);
  initContacts(token, currentUser.id);
  initMessaging(token, currentUser.id);
}

window.initChatPage = initChatPage;