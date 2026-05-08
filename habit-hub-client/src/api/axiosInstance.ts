import axios from 'axios';

const api = axios.create({
  baseURL: 'https://habithub-backend-dndna4fxckd9epby.polandcentral-01.azurewebsites.net',
});

api.interceptors.request.use((config) => {
  const sessionId = localStorage.getItem('sessionId');
  if (sessionId) {
    config.headers['X-Session-Id'] = sessionId;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      // Session expired or invalid — clear local auth state and redirect to login
      localStorage.removeItem('sessionId');
      localStorage.removeItem('userName');
      localStorage.removeItem('role');
      // Only redirect if not already on a public page
      if (!window.location.pathname.startsWith('/login') && !window.location.pathname.startsWith('/register')) {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export default api;
