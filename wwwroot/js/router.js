import { initLoginPage } from "./login.js";
import { initOtpPage } from "./otp.js";
import { initChatPage } from "./chat/chat.js";

const app = document.getElementById("app");

export async function loadPage(page) {
  try {
    const res = await fetch(`pages/${page}.html`);
    if (!res.ok) throw new Error(`Не удалось загрузить страницу: ${page}`);
    const html = await res.text();
    app.innerHTML = html;

    if (page === "login") {
      initLoginPage();
    } else if (page === "otp") {
      initOtpPage();
    } else if (page === "chat") {
      initChatPage();
    }
  } catch (err) {
    console.error("❌ Ошибка при загрузке страницы:", err);
    app.innerHTML = `<div class="error">Ошибка загрузки страницы: ${page}</div>`;
  }
}

const state = sessionStorage.getItem("step") || "login";
loadPage(state);