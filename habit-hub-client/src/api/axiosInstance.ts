import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5100',
});

api.interceptors.request.use((config) => {
  const sessionId = localStorage.getItem('sessionId');
  if (sessionId) {
    config.headers['Authorization'] = `Bearer ${sessionId}`;
  }
  return config;
});

export default api;