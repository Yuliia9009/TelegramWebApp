import { loadPage } from "./router.js";

export function initOtpPage() {
  const phone = sessionStorage.getItem("phone");
  const phoneDisplay = document.getElementById("phoneDisplay");
  if (phoneDisplay && phone) {
    phoneDisplay.textContent = phone;
  }

  // Кнопка подтверждения
  const verifyBtn = document.getElementById("verifyBtn");
  verifyBtn.onclick = () => {
    const code = document.getElementById("otpInput").value;

    fetch("/api/auth/verify-code", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ PhoneNumber: phone, Code: code }),
    })
      .then((res) => {
        if (!res.ok) throw new Error("Код неправильный");
        return res.json();
      })
      .then((data) => {
        sessionStorage.setItem("token", data.token);
        sessionStorage.setItem("user", JSON.stringify(data.user));
        loadPage("chat");
      })
      .catch((err) => alert("Ошибка: " + err.message));
  };

  // Кнопка повторной отправки кода
  const resendLink = document.getElementById("resendLink");
  if (resendLink) {
    resendLink.onclick = (e) => {
      e.preventDefault();
      fetch("/api/auth/send-code", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ phoneNumber: phone }),
      })
        .then((res) => {
          if (!res.ok) throw new Error("Не удалось отправить код повторно");
          return res.json();
        })
        .then(() => alert("Код отправлен повторно"))
        .catch((err) => alert("Ошибка: " + err.message));
    };
  }

  // Кнопка назад
  const backBtn = document.getElementById("backBtn");
  if (backBtn) {
    backBtn.onclick = () => {
      sessionStorage.setItem("step", "login");
      loadPage("login");
    };
  }

  // Анимация обезьянки
  const img = document.getElementById("logoMonkey");
  let isMonkey1 = true;

  if (img) {
    setInterval(() => {
      img.style.opacity = "0"; // скрыть

      setTimeout(() => {
        img.src = isMonkey1 ? "/media/monkey2.png" : "/media/monkey1.png";
        isMonkey1 = !isMonkey1;
        img.style.opacity = "1"; // показать
      }, 5); 
    }, 2000);
  }
}

window.initOtpPage = initOtpPage;