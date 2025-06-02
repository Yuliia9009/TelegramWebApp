import { initLoginPage } from "./login.js";
import { initOtpPage } from "./otp.js";
import { initChatPage } from "./chat/chat.js";

const app = document.getElementById("app");

export async function loadPage(page) {
  const res = await fetch(`pages/${page}.html`);
  const html = await res.text();
  app.innerHTML = html;

  if (page === "login") {
    initLoginPage();
  } else if (page === "otp") {
    initOtpPage();
  } else if (page === "chat") {
    initChatPage();
  }
}

const state = sessionStorage.getItem("step") || "login";
loadPage(state);