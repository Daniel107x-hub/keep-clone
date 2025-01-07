import {FormState, SignupFormSchema} from "@/app/lib/definitions";
import axios from "axios";

axios.defaults.baseURL = 'http://localhost:5085/api';

export async function signup(state: FormState, formData: FormData): Promise<FormState>{
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
    }catch (e: any) {
        console.log(e);
        return {errors: {request: e?.response.data}}
    }
}