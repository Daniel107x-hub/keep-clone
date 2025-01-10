import {SignupFormState, SignupFormSchema, LoginFormState, LoginFormSchema} from "@/app/lib/definitions";
import axios from "axios";
import api from "@/app/configs/axiosConfig";

axios.defaults.baseURL = 'http://localhost:50000/api';

export async function signup(state: SignupFormState, formData: FormData): Promise<SignupFormState>{
    const validateFields = SignupFormSchema.safeParse({
        firstName: formData.get('firstName'),
        lastName: formData.get('lastName'),
        userName: formData.get('userName'),
        phoneNumber: formData.get('phoneNumber'),
        email: formData.get('email'),
        password: formData.get('password')
    })
    if(!validateFields.success) return {
        errors: validateFields.error.flatten().fieldErrors,
        payload: formData

    }
    try{
        await api.post('/Account', validateFields.data);
        return {message: 'Account created successfully'}
    // @ts-expect-error Exception
    }catch (e: never) {
        return {
            errors: {request: e?.message},
            payload: formData
        }
    }
}

export async function login(state: LoginFormState, formData: FormData): Promise<LoginFormState>{
    const validateFields = LoginFormSchema.safeParse({
        userName: formData.get('userName'),
        password: formData.get('password')
    })
    if(!validateFields.success) return {
        errors: validateFields.error.flatten().fieldErrors
    }
    const { userName, password } = validateFields.data;
    const payload = {
        userName: userName,
        password: password
    }
    try{
        const response = await api.post('/Account/Login', payload);
        sessionStorage.setItem('token', response.data);
        return {message: 'Login successful'}
    // @ts-expect-error Exception
    }catch (e: never) {
        const status = e?.response.status;
        if(status >= 400 && status <= 499) return {errors: {request: 'Invalid username or password'}}
        return {
            errors: {request: e?.message},
            payload
        }
    }
}