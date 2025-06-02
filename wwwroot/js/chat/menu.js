export function initMenu(token) {
  const avatar = document.getElementById("menuAvatar");
  const dropdownMenu = document.getElementById("dropdownMenu");

  avatar?.addEventListener("click", (e) => {
    e.stopPropagation();
    dropdownMenu?.classList.toggle("show");
  });

  document.addEventListener("click", (e) => {
    if (!dropdownMenu?.contains(e.target)) {
      dropdownMenu?.classList.remove("show");
    }
  });

  document.getElementById("logout").onclick = () => {
    sessionStorage.clear();
    location.reload();
  };

  document.getElementById("addFriend").onclick = () => {
    const phone = prompt("Введите номер телефона друга (в формате +380...)");
    if (!phone || !phone.startsWith("+")) {
      alert("Некорректный номер.");
      return;
    }

    fetch("/api/friends/add", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: "Bearer " + token
      },
      body: JSON.stringify({ phoneNumber: phone })
    })
      .then(res => {
        if (!res.ok) throw new Error("Пользователь не найден или уже добавлен");
        return res.json();
      })
      .then(data => {
        return fetch("/api/chats/private", {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: "Bearer " + token
          },
          body: JSON.stringify({ participantId: data.friendId })
        });
      })
      .then(() => {
        alert("Друг добавлен!");
        location.reload();
      })
      .catch(err => alert("Ошибка: " + err.message));
  };
}