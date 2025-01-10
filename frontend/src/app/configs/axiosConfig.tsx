import axios from "axios";

const api = axios.create({
    baseURL: 'http://localhost:50000/api'
});

api.interceptors.request.use((config) => {
    const token = sessionStorage.getItem('token');
    if(token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});

api.interceptors.response.use((response) => {
    return response;
}, (error) => {
    const status = error.status;
    if (status >= 400 && status <= 499) {
        return Promise.reject({
            ...error,
            message: 'An error occurred'
        });
    }
    if (status >= 500 && status <= 599) {
        return Promise.reject({
            ...error,
            message: 'Server error, please try again later'
        });
    }
    return Promise.reject(error);
});

export default api;