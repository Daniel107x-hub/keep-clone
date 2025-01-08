import {useEffect, useState} from "react";
import api from "@/app/configs/axiosConfig";

function useAuth() {
    const [isAuthenticated, setIsAuthenticated] = useState(false);
    const [isLoading, setIsLoading] = useState(true);
    useEffect(() => {
        const token = sessionStorage.getItem('token');
        if(!token){
            setIsAuthenticated(false);
            setIsLoading(false);
            return;
        }
        api.get('/Account/CurrentUser').then((response) => {
            if(response.status === 200) {
                setIsAuthenticated(true);
            }else{
                sessionStorage.removeItem('token');
                setIsAuthenticated(false);
            }
        }).catch(() => {
            sessionStorage.removeItem('token');
            setIsAuthenticated(false);
        }).finally(() => {
            setIsLoading(false);
        })
    }, []);

    return [
        isAuthenticated,
        isLoading
    ]
}

export default useAuth;