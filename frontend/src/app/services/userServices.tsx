import api from "@/app/configs/axiosConfig";

async function isUserLoggedIn(): Promise<boolean> {
    const token = sessionStorage.getItem('token');
    if(!token) return false;
    const response = await api.get('/Account/CurrentUser');
    if(response.status === 200) return true;
    sessionStorage.removeItem('token');
    return false;

}

export {
    isUserLoggedIn
}