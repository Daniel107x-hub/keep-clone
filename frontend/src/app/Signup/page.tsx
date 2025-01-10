'use client'
import { signup } from '@/app/actions/auth';
import { useActionState, useEffect} from "react";
import {toast, ToastContainer} from "react-toastify";
import Input from "@/app/components/Input/Input";
import useAuth from "@/app/hooks/useAuth";
import {redirect} from "next/navigation";
import Link from "next/link";
import Loader from "@/app/components/Loader/Loader";


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
        id: "phoneNumber",
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
    const [isAuthenticated, isLoading] = useAuth();
    useEffect(() => {
        if(pending) return;
        if(state?.message) {
            toast.success(state.message);
        }
        if(state?.errors?.request) {
            toast.error(state.errors.request);
        }
    }, [state, pending])
    if(isLoading) return <Loader fullscreen/>
    if(isAuthenticated) redirect('/');
    return (
        <div className="w-full h-screen flex flex-col gap-4 items-center justify-center">
            <form action={action}
                  className='flex flex-col gap-4 items-center py-4 bg-slate-800 p-8 rounded-lg w-1/2 md:w-1/4 xl:w-1/5'>
                <h1 className="text-xl font-bold">Register</h1>
                {INPUTS.map((input) => {
                    const errors: string | string[] | undefined = state?.errors ? state?.errors[input.id as keyof typeof state.errors] : undefined;
                    const payload: FormData = state?.payload as FormData;
                    const value: string | number | undefined = payload?.get(input.id) as (string | number) || undefined;
                    return (
                        <div className='flex flex-col items-start w-full' key={input.id}>
                            <Input {...input} defaultValue={value}/>
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
                <button disabled={pending} type="submit"
                        className="bg-amber-300 p-3 rounded hover:bg-amber-500 text-amber-800">Sign
                    up
                </button>
            </form>
            <h4 className="text-xs">Already have an account? Click <Link href="/Login"
                                                     className="text-amber-300 hover:text-amber-500">here</Link> to
                login!</h4>
            <ToastContainer/>
        </div>
    )
}

export default SignupForm;