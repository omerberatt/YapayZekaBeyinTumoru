const container = document.getElementById('container');
const registerBtn = document.getElementById('register');
const loginBtn = document.getElementById('login');

registerBtn.addEventListener('click', () => {
  container.classList.add('active');
});

loginBtn.addEventListener('click', () => {
  container.classList.remove('active');
});

document.addEventListener('DOMContentLoaded', () => {
  const registerForm = document.getElementById('registerForm');
  const loginForm = document.getElementById('loginForm');

  registerForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    await registerUser();
  });

  loginForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    await loginUser();
  });
});

async function registerUser() {
  const name = document.getElementById('registerName').value;
  const email = document.getElementById('registerEmail').value;
  const password = document.getElementById('registerPassword').value;

  const response = await fetch('https://localhost:7100/api/Register', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ name, email, password })
  });

  const messageDiv = document.getElementById('message');
  if (response.ok) {
    messageDiv.innerHTML = `<p class="success">Kayıt başarılı!</p>`;
    // Kayıt başarılı olduğunda giriş formuna geç

    container.classList.remove('active');
  } else {
    const errorData = await response.json();
    messageDiv.innerHTML = `<p class="error">Kayıt başarısız: ${errorData.message}</p>`;
  }
}
async function loginUser() {
  const email = document.getElementById('loginEmail').value;
  const password = document.getElementById('loginPassword').value;

  const response = await fetch('https://localhost:7100/api/auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ email, password })
  });

  const messageDiv = document.getElementById('message');
  if (response.ok) {
    const data = await response.json();

    // Kullanıcı bilgilerini localStorage'a kaydet
    localStorage.setItem('userId', data.userId); // API'den dönen `userId` değerini saklıyoruz
    localStorage.setItem('token', data.token); // Eğer token dönerse bunu da saklıyoruz
    localStorage.setItem('userName', data.name); // Kullanıcının ismini de saklayabilirsiniz

    // Bilgileri console.log ile kontrol et
    console.log(`User ID: ${data.userId}, User Name: ${data.name}`);

    messageDiv.innerHTML = `<p class="success">Giriş başarılı!</p>`;

    // Giriş başarılı olduğunda index.html sayfasına yönlendir
    setTimeout(() => {
      window.location.href = 'index.html';
    }, 2000); // 2 saniye sonra yönlendirme
  } else {
    const errorData = await response.json();
    messageDiv.innerHTML = `<p class="error">Giriş başarısız: ${errorData.message}</p>`;
    // Giriş başarısız olduğunda kayıt formuna geç
    container.classList.add('active');
  }
}
