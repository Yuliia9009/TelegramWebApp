import { initLoginPage } from "./login.js";
import { initOtpPage } from "./otp.js";
import { initChatPage } from "./chat/chat.js";

const app = document.getElementById("app");

export async function loadPage(page, data = null) {
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
      initChatPage(data);

      // ⏳ Ждём, пока появится элемент #messages
      const waitForMessages = () =>
        new Promise(resolve => {
          const check = () => {
            if (document.getElementById("messages")) resolve();
            else setTimeout(check, 30);
          };
          check();
        });

      await waitForMessages();

      if (data?.userId && typeof window.openChat === "function") {
        window.openChat(data.userId, true);
      }
    }
  } catch (err) {
    console.error("❌ Ошибка при загрузке страницы:", err);
    app.innerHTML = `<div class="error">Ошибка загрузки страницы: ${page}</div>`;
  }
}

const state = sessionStorage.getItem("step") || "login";
loadPage(state);