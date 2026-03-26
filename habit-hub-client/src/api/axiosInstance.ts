import axios from 'axios';

const api = axios.create({
  baseURL: 'https://habithub-backend-dndna4fxckd9epby.polandcentral-01.azurewebsites.net',
});

api.interceptors.request.use((config) => {
  const sessionId = localStorage.getItem('sessionId');
  if (sessionId) {
    config.headers['Authorization'] = `Bearer ${sessionId}`;
  }
  return config;
});

export default api;
