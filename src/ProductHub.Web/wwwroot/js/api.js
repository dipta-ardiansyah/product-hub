// api.js - Centralized fetch wrapper for API calls
const Api = {
    async request(endpoint, options = {}, requiresAuth = true) {
        const headers = {
            'Content-Type': 'application/json',
            ...(options.headers || {})
        };

        if (requiresAuth) {
            const token = Auth.getToken();
            if (!token) {
                Auth.logout();
                throw new Error('Authentication required.');
            }
            headers['Authorization'] = `Bearer ${token}`;
        }

        const config = {
            ...options,
            headers
        };

        const response = await fetch(endpoint, config);

        if (response.status === 401) {
            Auth.logout();
            throw new Error('Session expired or unauthorized. Redirecting to login...');
        }

        if (response.status === 204) {
            return null;
        }

        const contentType = response.headers.get('content-type');
        let data = null;
        if (contentType && contentType.includes('application/json') || contentType && contentType.includes('application/problem+json')) {
            data = await response.json();
        }

        if (!response.ok) {
            let errorMessage = 'An error occurred.';
            if (data) {
                if (data.errors && typeof data.errors === 'object') {
                    const messages = [];
                    for (const key in data.errors) {
                        messages.push(...data.errors[key]);
                    }
                    errorMessage = messages.join(' ');
                } else if (data.detail) {
                    errorMessage = data.detail;
                } else if (data.title) {
                    errorMessage = data.title;
                }
            }
            const error = new Error(errorMessage);
            error.status = response.status;
            error.data = data;
            throw error;
        }

        return data;
    },

    get(endpoint, requiresAuth = true) {
        return this.request(endpoint, { method: 'GET' }, requiresAuth);
    },

    post(endpoint, body, requiresAuth = true) {
        return this.request(endpoint, {
            method: 'POST',
            body: JSON.stringify(body)
        }, requiresAuth);
    },

    put(endpoint, body, requiresAuth = true) {
        return this.request(endpoint, {
            method: 'PUT',
            body: JSON.stringify(body)
        }, requiresAuth);
    },

    delete(endpoint, requiresAuth = true) {
        return this.request(endpoint, { method: 'DELETE' }, requiresAuth);
    }
};
