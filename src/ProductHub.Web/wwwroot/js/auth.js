// auth.js - Authentication and token management in sessionStorage
const Auth = {
    TOKEN_KEY: 'producthub_jwt_token',
    EMAIL_KEY: 'producthub_user_email',

    getToken() {
        return sessionStorage.getItem(this.TOKEN_KEY);
    },

    getUserEmail() {
        return sessionStorage.getItem(this.EMAIL_KEY);
    },

    setToken(token, email) {
        sessionStorage.setItem(this.TOKEN_KEY, token);
        if (email) {
            sessionStorage.setItem(this.EMAIL_KEY, email);
        }
        this.renderNav();
    },

    isAuthenticated() {
        return !!this.getToken();
    },

    logout() {
        sessionStorage.removeItem(this.TOKEN_KEY);
        sessionStorage.removeItem(this.EMAIL_KEY);
        this.renderNav();
        if (window.location.pathname !== '/Account/Login' && window.location.pathname !== '/Account/Register') {
            window.location.href = '/Account/Login';
        }
    },

    renderNav() {
        const authNav = document.getElementById('authNav');
        if (!authNav) return;

        if (this.isAuthenticated()) {
            const email = this.getUserEmail() || 'User';
            authNav.innerHTML = `
                <li class="nav-item dropdown">
                    <a class="nav-link dropdown-toggle text-light" href="#" role="button" data-bs-toggle="dropdown">
                        <i class="bi bi-person-circle me-1"></i> ${this.escapeHtml(email)}
                    </a>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li><a class="dropdown-item" href="javascript:void(0)" onclick="Auth.logout()"><i class="bi bi-box-arrow-right me-2 text-danger"></i>Logout</a></li>
                    </ul>
                </li>
            `;
        } else {
            authNav.innerHTML = `
                <li class="nav-item">
                    <a class="nav-link" href="/Account/Login">Login</a>
                </li>
                <li class="nav-item">
                    <a class="btn btn-primary btn-sm ms-2 mt-1" href="/Account/Register">Register</a>
                </li>
            `;
        }
    },

    escapeHtml(str) {
        if (!str) return '';
        return str.replace(/&/g, '&amp;')
                  .replace(/</g, '&lt;')
                  .replace(/>/g, '&gt;')
                  .replace(/"/g, '&quot;')
                  .replace(/'/g, '&#039;');
    }
};

document.addEventListener('DOMContentLoaded', () => {
    Auth.renderNav();
});
