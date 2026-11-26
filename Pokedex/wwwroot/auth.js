/* Pokedex auth gate: nobody sees the Pokedex until they log in or create an account.
 * Uses the HttpOnly cookie set by the server, so there is NO token in JavaScript or localStorage.
 * The login screen has an animated background (Pokémon running/floating, drifting Poké Balls).
 *
 * In wwwroot/index.html:
 *   <link rel="stylesheet" href="auth.css">          (in <head>)
 *   <script src="auth.js"></script>                  (BEFORE your main <script>)
 */
(() => {
  'use strict';

  const root = document.documentElement;
  root.classList.add('auth-locked');      // hide the app until the server confirms we're logged in

  const nativeFetch = window.fetch.bind(window);
  let mode = 'login';
  let currentUser = null;                 // { username } or null
  let gateShown = false;

  // Sprites for the login background (same public sprite set most Pokédex apps use).
  // If an image can't load, that Pokémon is simply skipped.
  const SPRITE_BASE = 'https://raw.githubusercontent.com/PokeAPI/sprites/master/sprites/pokemon/';
  const SPRITES = [
    { id: 25,  kind: 'run'   },   // Pikachu
    { id: 4,   kind: 'run'   },   // Charmander
    { id: 7,   kind: 'run'   },   // Squirtle
    { id: 1,   kind: 'run'   },   // Bulbasaur
    { id: 39,  kind: 'run'   },   // Jigglypuff
    { id: 133, kind: 'run'   },   // Eevee
    { id: 92,  kind: 'float' },   // Gastly
    { id: 41,  kind: 'float' },   // Zubat
    { id: 12,  kind: 'float' },   // Butterfree
    { id: 151, kind: 'float' }    // Mew
  ];

  // If any protected /api call comes back 401 (not logged in / session expired), show the gate.
  window.fetch = async (input, init) => {
    const res = await nativeFetch(input, init);
    try {
      const raw = input instanceof Request ? input.url : String(input);
      const u = new URL(raw, location.href);
      const isApi = u.origin === location.origin && u.pathname.startsWith('/api/') && !u.pathname.startsWith('/api/auth/');
      if (isApi && res.status === 401) {
        showGate('login', currentUser ? 'Session expired. Please log in again.' : '');
        currentUser = null;
      }
    } catch { /* ignore */ }
    return res;
  };

  const $ = (id) => document.getElementById(id);
  const rand = (min, max) => Math.random() * (max - min) + min;

  const MODAL_HTML = `
    <div id="auth-bar"></div>
    <div id="auth-overlay" hidden>
      <div id="auth-bg" aria-hidden="true"></div>
      <div class="auth-box" role="dialog" aria-modal="true" aria-labelledby="auth-title">
        <h2 id="auth-title">TRAINER LOGIN</h2>
        <p class="auth-sub">Log in or create an account to open the Pokédex.</p>
        <form id="auth-form" novalidate>
          <label><span id="lbl-user">USERNAME OR EMAIL</span>
            <input id="auth-user" autocomplete="username" maxlength="100"></label>
          <label id="row-email" hidden>EMAIL
            <input id="auth-email" type="email" autocomplete="email" maxlength="100"></label>
          <label>PASSWORD
            <input id="auth-pass" type="password" autocomplete="current-password" maxlength="100"></label>
          <label id="row-confirm" hidden>CONFIRM PASSWORD
            <input id="auth-confirm" type="password" autocomplete="new-password" maxlength="100"></label>
          <p id="auth-msg" role="alert" aria-live="polite"></p>
          <button type="submit" id="auth-submit" class="auth-btn">LOGIN</button>
          <button type="button" id="auth-switch" class="auth-link"></button>
        </form>
      </div>
    </div>`;

  // ---------- animated background ----------
  function buildBackground() {
    const bg = $('auth-bg');
    if (!bg || bg.dataset.built) return;
    bg.dataset.built = '1';
    const frag = document.createDocumentFragment();

    for (let i = 0; i < 30; i++) {                    // twinkling stars
      const s = document.createElement('span');
      s.className = 'pstar';
      s.style.left = rand(0, 100) + '%';
      s.style.top = rand(0, 72) + '%';
      s.style.animationDuration = rand(2, 5) + 's';
      s.style.animationDelay = rand(-5, 0) + 's';
      frag.append(s);
    }

    for (let i = 0; i < 9; i++) {                     // Poké Balls floating up
      const b = document.createElement('span');
      const size = rand(26, 62);
      b.className = 'pball';
      b.style.width = b.style.height = size + 'px';
      b.style.left = rand(2, 96) + '%';
      b.style.animationDuration = rand(14, 28) + 's';
      b.style.animationDelay = rand(-28, 0) + 's';    // negative = already mid-flight on load
      frag.append(b);
    }

    const ground = document.createElement('div');
    ground.className = 'pground';
    frag.append(ground);

    SPRITES.forEach((p, i) => {                       // Pokémon
      const wrap = document.createElement('div');
      const img = new Image();
      img.src = SPRITE_BASE + p.id + '.png';
      img.alt = '';
      img.draggable = false;
      img.decoding = 'async';
      img.addEventListener('error', () => wrap.remove());

      if (p.kind === 'run') {
        const dir = i % 2 === 0 ? 'right' : 'left';
        wrap.className = 'psprite run ' + dir;
        wrap.style.bottom = rand(1, 7) + 'vh';
        wrap.style.animationDuration = rand(14, 26) + 's';
        wrap.style.animationDelay = rand(-26, 0) + 's';
        img.style.animationDelay = rand(-1, 0) + 's';
      } else {
        wrap.className = 'psprite float';
        wrap.style.left = rand(5, 88) + '%';
        wrap.style.top = rand(8, 55) + '%';
        wrap.style.animationDuration = rand(9, 16) + 's';
        wrap.style.animationDelay = rand(-16, 0) + 's';
      }
      wrap.append(img);
      frag.append(wrap);
    });

    bg.append(frag);
  }

  function setMsg(text, ok = false) {
    const m = $('auth-msg');
    m.textContent = text;                 // textContent only, never innerHTML
    m.classList.toggle('ok', ok);
  }

  function setMode(m, note = '') {
    mode = m;
    const reg = m === 'register';
    $('auth-title').textContent = reg ? 'NEW TRAINER' : 'TRAINER LOGIN';
    $('lbl-user').textContent = reg ? 'USERNAME' : 'USERNAME OR EMAIL';
    $('row-email').hidden = !reg;
    $('row-confirm').hidden = !reg;
    $('auth-pass').autocomplete = reg ? 'new-password' : 'current-password';
    $('auth-submit').textContent = reg ? 'CREATE ACCOUNT' : 'LOGIN';
    $('auth-switch').textContent = reg ? 'Already have an account? LOGIN' : 'No account yet? CREATE ONE';
    setMsg(note);
  }

  function showGate(m = 'login', note = '') {
    if (gateShown || !$('auth-overlay')) return;      // not built yet -> checkSession() will show it
    gateShown = true;
    root.classList.add('auth-locked');
    buildBackground();
    $('auth-form').reset();
    setMode(m, note);
    $('auth-overlay').hidden = false;
    $('auth-user').focus();
  }

  function unlock() {
    gateShown = false;
    $('auth-overlay').hidden = true;
    root.classList.remove('auth-locked');
  }

  function makeButton(label, onClick) {
    const b = document.createElement('button');
    b.type = 'button';
    b.textContent = label;
    b.addEventListener('click', onClick);
    return b;
  }

  function renderBar() {
    document.body.classList.toggle('logged-in', !!currentUser);
    const bar = $('auth-bar');
    bar.replaceChildren();
    if (currentUser) {
      const who = document.createElement('span');
      who.className = 'auth-who';
      who.textContent = '▶ ' + currentUser.username;   // username is user input -> textContent only
      bar.append(who, makeButton('LOGOUT', logout));
    }
    window.dispatchEvent(new CustomEvent('auth-changed', { detail: { user: currentUser } }));
  }

  async function logout() {
    try { await nativeFetch('/api/auth/logout', { method: 'POST' }); } catch { /* ignore */ }
    location.reload();                    // back to the login gate
  }

  async function checkSession() {
    try {
      const res = await nativeFetch('/api/auth/me');
      const data = await res.json();
      if (data.authenticated) {
        currentUser = { username: data.username };
        renderBar();
        unlock();
      } else {
        currentUser = null;
        renderBar();
        showGate('login');
      }
    } catch {
      renderBar();
      showGate('login', 'Cannot reach the server. Try again.');
    }
  }

  async function onSubmit(e) {
    e.preventDefault();
    const reg = mode === 'register';
    const user = $('auth-user').value.trim();
    const email = $('auth-email').value.trim();
    const pass = $('auth-pass').value;
    const confirm = $('auth-confirm').value;

    if (!user || !pass) return setMsg('Please fill in all fields.');
    if (reg) {
      if (user.length < 3 || user.length > 30) return setMsg('Username must be 3-30 characters.');
      if (!/^[A-Za-z0-9_.-]+(?: [A-Za-z0-9_.-]+)*$/.test(user))
        return setMsg('Username can only have letters, numbers, single spaces, and _ . -');
      if (!/^\S+@\S+\.\S+$/.test(email)) return setMsg('Enter a valid email address.');
      if (pass.length < 8) return setMsg('Password must be at least 8 characters.');
      if (!/[a-z]/.test(pass) || !/\d/.test(pass)) return setMsg('Password needs a lowercase letter and a number.');
      if (pass !== confirm) return setMsg('Passwords do not match.');
    }

    const btn = $('auth-submit');
    btn.disabled = true;
    setMsg('Please wait...', true);
    try {
      const res = await nativeFetch(reg ? '/api/auth/register' : '/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(reg
          ? { username: user, email, password: pass }
          : { usernameOrEmail: user, password: pass, rememberMe: true })
      });
      const data = await res.json().catch(() => ({}));
      if (!res.ok) {
        const validation = data && data.errors ? Object.values(data.errors).flat()[0] : '';
        setMsg(data.message || validation || 'Something went wrong. Try again.');
        btn.disabled = false;
        return;
      }
      // Logged in (cookie is set). Reload so the Pokédex loads its data as a logged-in user.
      location.reload();
    } catch {
      setMsg('Cannot reach the server. Try again.');
      btn.disabled = false;
    }
  }

  function init() {
    document.body.insertAdjacentHTML('beforeend', MODAL_HTML);
    $('auth-form').addEventListener('submit', onSubmit);
    $('auth-switch').addEventListener('click', () => {
      $('auth-form').reset();
      setMode(mode === 'login' ? 'register' : 'login');
    });
    checkSession();
  }

  window.Auth = {
    isLoggedIn: () => !!currentUser,
    user: () => currentUser,
    logout
  };

  if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', init);
  else init();
})();