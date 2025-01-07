'use client'
import { signup } from '@/app/actions/auth';
import { useActionState, useEffect} from "react";
import {toast, ToastContainer} from "react-toastify";

const InputField = ({ label, id, placeholder, type}: { label: string, id: string, placeholder:string, type: string}) => {
    return <div className="flex flex-col w-full py-2 gap-1">
        <label htmlFor={id} className="text-xs">{label}</label>
        <input id={id} name={id} placeholder={placeholder} type={type} className="w-full bg-slate-600 rounded text-slate-100 px-1"/>
    </div>
}

// Auth steps:
// 1. Create form -> ui/SignupForm
// 2. Validate fields on both client and server -> lib/definitions/SignupFormSchema
// 3. Execute registration -> actions/auth/Signup

const INPUTS = [
    {
        label: "First name",
        id: "firstName",
        placeholder: "First Name",
        type: "text"
    },
    {
        label: "Last name",
        id: "lastName",
        placeholder: "Last Name",
        type: "text"
    },
    {
        label: "Username",
        id: "userName",
        placeholder: "Username",
        type: "text"
    },
    {
        label: "Phone",
        id: "phone",
        placeholder: "Phone",
        type: "text"
    },
    {
        label: "Email",
        id: "email",
        placeholder: "Email",
        type: "email"
    },
    {
        label: "Password",
        id: "password",
        placeholder: "Password",
        type: "password"
    }
]

const SignupForm = () => {
    const [state, action, pending] = useActionState(signup, undefined);
    useEffect(() => {
        if(state?.message) {
            toast.success(state.message);
        }
        if(state?.errors?.request) {
            toast.error(state.errors.request);
        }
    }, [state])
    return (
        <div className="w-full h-screen flex items-center justify-center">
            <form action={action} className='flex flex-col gap-4 items-center py-4 bg-slate-800 p-8 rounded-lg w-1/2 md:w-1/4 xl:w-1/5'>
                <h1 className="text-xl font-bold">Register</h1>
                {INPUTS.map((input) => {
                    const errors: string | string[] | undefined = state?.errors ? state?.errors[input.id as keyof typeof state.errors] : undefined;
                    return (
                        <div className='flex flex-col items-start w-full' key={input.id}>
                            <InputField {...input}/>
                            {
                                errors && Array.isArray(errors) &&
                                <ul className="text-xs text-red-400 list-disc">
                                    {errors.map((error: string) => (
                                        <li key={error}>{error}</li>
                                    ))}
                                </ul>
                            }
                        </div>
                    )
                })}
                <button disabled={pending} type="submit" className="bg-amber-300 p-3 rounded hover:bg-amber-500 text-amber-800">Sign
                    up
                </button>
            </form>
            <ToastContainer/>
        </div>
    )
}

export default SignupForm;