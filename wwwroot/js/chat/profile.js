export function initProfile(token) {
  document.getElementById("editProfile").onclick = openProfileModal;
  document.getElementById("closeProfileModal").onclick = () =>
    document.getElementById("profileModal").classList.remove("show");

  document.getElementById("saveProfileBtn").onclick = () => {
  const nickname = document.getElementById("nicknameInput").value;
  const birthdate = document.getElementById("birthdateInput").value;
  const user = JSON.parse(sessionStorage.getItem("user") || "{}");
  const avatarFile = document.getElementById("avatarInput").files[0];

  // Если выбран новый файл аватара — загружаем его
  if (avatarFile) {
    const formData = new FormData();
    formData.append("file", avatarFile);

    fetch("/api/users/me/avatar", {
      method: "POST",
      headers: {
        Authorization: "Bearer " + token
      },
      body: formData
    })
      .then(res => {
        if (!res.ok) throw new Error("Ошибка загрузки аватара");
        return res.json();
      })
      .then(data => {
        updateUserProfile(nickname, birthdate, data.avatarUrl);
      })
      .catch(err => alert("Ошибка загрузки аватара: " + err.message));
  } else {
    updateUserProfile(nickname, birthdate, null);
  }
};

function updateUserProfile(nickname, birthdate, avatarUrl) {
  const user = JSON.parse(sessionStorage.getItem("user") || "{}");

  const body = {
    nickname,
    dateOfBirth: birthdate
  };

  if (avatarUrl) body.avatarUrl = avatarUrl;

  fetch("/api/users/me", {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: "Bearer " + token
    },
    body: JSON.stringify(body)
  })
    .then(res => {
      if (!res.ok) throw new Error("Ошибка обновления профиля");
      return res.json();
    })
    .then(updated => {
      alert("Профиль обновлён");
      sessionStorage.setItem("user", JSON.stringify(updated));
      document.getElementById("profileModal").classList.remove("show");
    })
    .catch(err => alert("Ошибка: " + err.message));
}

  function openProfileModal() {
    const user = JSON.parse(sessionStorage.getItem("user") || "{}");
    document.getElementById("nicknameInput").value = user.nickname || "";
    document.getElementById("birthdateInput").value = user.birthdate || "";
    document.getElementById("phoneInputEdit").value = user.phoneNumber || "";
    document.getElementById("avatarPreview").src = "img/default-avatar.png";
    document.getElementById("profileModal").classList.add("show");
  }
}