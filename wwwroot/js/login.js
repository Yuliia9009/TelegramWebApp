import { initCountries } from "./countries.js";
import { loadPage } from "./router.js";

export function initLoginPage() {
  initCountries("countrySelect");

  const sendCodeBtn = document.getElementById("sendCodeBtn");
  if (!sendCodeBtn) return;

  sendCodeBtn.onclick = () => {
    const countryCode = document.getElementById("countrySelect").value;
    const phone = document.getElementById("phoneInput").value;
    const fullPhone = `${countryCode}${phone.replace(/\D/g, "")}`;
    sessionStorage.setItem("phone", fullPhone);

    fetch("/api/auth/send-code", {
      method: "POST",
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ phoneNumber: fullPhone })
    })
      .then(async res => {
        if (!res.ok) {
          const text = await res.text();
          alert("Ошибка: " + text);
        } else {
          alert("Код отправлен");
          sessionStorage.setItem("step", "otp");
          loadPage("otp");
        }
      })
      .catch(err => alert("Сетевая ошибка: " + err.message));
  };
}

window.initLoginPage = initLoginPage;