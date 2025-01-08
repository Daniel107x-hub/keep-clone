import {SignupFormState, SignupFormSchema, LoginFormState, LoginFormSchema} from "@/app/lib/definitions";
import axios from "axios";

axios.defaults.baseURL = 'http://localhost:50000/api';

export async function signup(state: SignupFormState, formData: FormData): Promise<SignupFormState>{
    const validateFields = SignupFormSchema.safeParse({
        firstName: formData.get('firstName'),
        lastName: formData.get('lastName'),
        userName: formData.get('userName'),
        phone: formData.get('phone'),
        email: formData.get('email'),
        password: formData.get('password')
    })
    if(!validateFields.success) return {
        errors: validateFields.error.flatten().fieldErrors
    }
    const { firstName, lastName, userName, phone, email, password } = validateFields.data;
    const payload = {
        firstName: firstName,
        lastName: lastName,
        userName: userName,
        phoneNumber: phone,
        email: email,
        password: password
    }
    try{
        await axios.post('/Account', payload);
        return {message: 'Account created successfully'}
    // @ts-expect-error Exception
    }catch (e: never) {
        return {errors: {request: e?.response.data}}
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
        const response = await axios.post('/Account/Login', payload);
        sessionStorage.setItem('token', response.data);
        return {message: 'Login successful'}
    // @ts-expect-error Exception
    }catch (e: never) {
        return {errors: {request: e?.response.data}}
    }
}