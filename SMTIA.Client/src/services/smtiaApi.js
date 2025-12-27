import { apiRequest, unwrapResult } from './apiClient';

export const smtiaApi = {
    public: {
        async isEmailAvailable(email) {
            return apiRequest(`/api/public/email-available?email=${encodeURIComponent(email)}`, { auth: false });
        },
        async searchMedicines(query, limit = 10) {
            const res = await apiRequest(`/api/public/medicines/search?query=${encodeURIComponent(query)}&limit=${limit}`, { auth: false });
            return unwrapResult(res);
        }
    },
    auth: {
        async login(emailOrUserName, password) {
            const res = await apiRequest('/api/auth/login', {
                method: 'POST',
                auth: false,
                body: { emailOrUserName, password }
            });
            return unwrapResult(res);
        },

        async register(payload) {
            const res = await apiRequest('/api/auth/register', {
                method: 'POST',
                auth: false,
                body: payload
            });
            return unwrapResult(res);
        },

        async registerOnboarding(payload) {
            const res = await apiRequest('/api/onboarding/register', {
                method: 'POST',
                auth: false,
                body: payload
            });
            // Backend returns direct JSON (not TS.Result) for onboarding
            // Check if it's TS.Result format, otherwise return as-is
            if (res && typeof res === 'object' && 'isSuccessful' in res) {
                return unwrapResult(res);
            }
            return res;
        },

        async forgotPassword(email) {
            const res = await apiRequest('/api/auth/forgot-password', {
                method: 'POST',
                auth: false,
                body: { email }
            });
            return unwrapResult(res);
        },

        async resetPassword(payload) {
            const res = await apiRequest('/api/auth/reset-password', {
                method: 'POST',
                auth: false,
                body: payload
            });
            // Backend returns direct JSON (not TS.Result) for reset password
            return res;
        }
    },

    userMedicines: {
        async list() {
            return apiRequest('/api/usermedicines');
        },
        async add(payload) {
            return apiRequest('/api/usermedicines', { method: 'POST', body: payload });
        },
        async update(scheduleId, payload) {
            return apiRequest(`/api/usermedicines/${scheduleId}`, { method: 'PUT', body: payload });
        },
        async remove(scheduleId) {
            return apiRequest(`/api/usermedicines/${scheduleId}`, { method: 'DELETE' });
        }
    },

    chat: {
        async send(message) {
            return apiRequest('/api/chat', { method: 'POST', body: { message } });
        },
        async getBmiAnalysis() {
            return apiRequest('/api/chat/bmi-analysis', { method: 'POST' });
        },
        async getInteractionAnalysis() {
            return apiRequest('/api/chat/interaction-analysis', { method: 'POST' });
        }
    },

    profile: {
        async me() {
            return apiRequest('/api/profile/me');
        },
        async update(payload) {
            return apiRequest('/api/profile/me', { method: 'PUT', body: payload });
        },
        async getHealth() {
            return apiRequest('/api/profile/health');
        },
        async updateHealth(payload) {
            return apiRequest('/api/profile/health', { method: 'PUT', body: payload });
        }
    },

    sideEffects: {
        async list() {
            return apiRequest('/api/sideeffects');
        },
        async add(payload) {
            return apiRequest('/api/sideeffects', { method: 'POST', body: payload });
        },
        async remove(id) {
            return apiRequest(`/api/sideeffects/${id}`, { method: 'DELETE' });
        }
    },

    medicines: {
        async getAll() {
            const res = await apiRequest('/api/medicines');
            return unwrapResult(res);
        },
        async searchLocal(query, limit = 10) {
            const res = await apiRequest(`/api/medicines/search?query=${encodeURIComponent(query)}&limit=${limit}`);
            return unwrapResult(res);
        },
        async searchFda(searchTerm, limit = 10) {
            const res = await apiRequest(`/api/medicines/fda/search?searchTerm=${encodeURIComponent(searchTerm)}&limit=${limit}`);
            return unwrapResult(res);
        },
        async getFdaDetails(labelId) {
            const res = await apiRequest(`/api/medicines/fda/${encodeURIComponent(labelId)}`);
            return unwrapResult(res);
        },
        async create(payload) {
            const res = await apiRequest('/api/medicines', { method: 'POST', body: payload });
            return unwrapResult(res);
        },
        async update(id, payload) {
            const res = await apiRequest(`/api/medicines/${id}`, { method: 'PUT', body: payload });
            return unwrapResult(res);
        },
        async delete(id) {
            const res = await apiRequest(`/api/medicines/${id}`, { method: 'DELETE' });
            return unwrapResult(res);
        }
    },

    admin: {
        async getUsers() {
            return apiRequest('/api/admin/users');
        },
        async getRoles() {
            return apiRequest('/api/admin/roles');
        },
        async setUserRoles(userId, roles) {
            return apiRequest(`/api/admin/users/${userId}/roles`, {
                method: 'PUT',
                body: { roles }
            });
        },
        async getAuditLogs(params = {}) {
            const queryParams = new URLSearchParams();
            if (params.startDate) queryParams.append('startDate', params.startDate);
            if (params.endDate) queryParams.append('endDate', params.endDate);
            if (params.action) queryParams.append('action', params.action);
            if (params.entityType) queryParams.append('entityType', params.entityType);
            if (params.userId) queryParams.append('userId', params.userId);
            if (params.page) queryParams.append('page', params.page);
            if (params.pageSize) queryParams.append('pageSize', params.pageSize);
            return apiRequest(`/api/admin/audit-logs?${queryParams.toString()}`);
        },
        async getSerilogLogs(lines = 1000) {
            return apiRequest(`/api/admin/serilog-logs?lines=${lines}`);
        },
        async deleteUser(userId) {
            return apiRequest(`/api/admin/users/${userId}`, { method: 'DELETE' });
        },
        async toggleUserLockout(userId, lockoutEnabled) {
            return apiRequest(`/api/admin/users/${userId}/lockout`, {
                method: 'PUT',
                body: { lockoutEnabled }
            });
        },
        async getStats() {
            return apiRequest('/api/admin/stats');
        }
    }
};


