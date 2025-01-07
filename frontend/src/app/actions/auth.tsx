import {FormState, SignupFormSchema} from "@/app/lib/definitions";

export async function signup(state: FormState, formData: FormData){
    const validateFields = SignupFormSchema.safeParse({
        firstName: formData.get('firstName'),
        lastName: formData.get('lastName'),
        email: formData.get('email'),
        password: formData.get('password')
    })
    if(!validateFields.success) return {
        errors: validateFields.error.flatten().fieldErrors
    }
    const data = validateFields.data;
    console.log("Successfully registered", data);
}