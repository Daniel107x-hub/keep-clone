"use client"
import {useActionState, useEffect} from "react";
import {login} from "@/app/actions/auth";
import {toast, ToastContainer} from "react-toastify";
import Input from "@/app/components/Input";
import {redirect} from "next/navigation";
import useAuth from "@/app/hooks/useAuth";

const INPUTS = [
    {
        label: "Username",
        id: "userName",
        placeholder: "Username",
        type: "text"
    },
    {
        label: "Password",
        id: "password",
        placeholder: "Password",
        type: "password"
    }
]

const LoginForm = () => {
    const [state, action, pending] = useActionState(login, undefined);
    const [isAuthenticated, isLoading] = useAuth();
    useEffect(() => {
        if(state?.message) {
            toast.success(state.message);
            redirect('/');
        }
        if(state?.errors?.request) {
            toast.error(state.errors.request);
        }
    }, [state])
    if(isLoading) return <>Loading...</>
    if(isAuthenticated) redirect('/');
    return (
        <div className="w-full h-screen flex items-center justify-center">
            <form action={action} className='flex flex-col gap-4 items-center py-4 bg-slate-800 p-8 rounded-lg w-1/2 md:w-1/4 xl:w-1/5'>
                <h1 className="text-xl font-bold">Login</h1>
                {INPUTS.map((input) => {
                    const errors: string | string[] | undefined = state?.errors ? state?.errors[input.id as keyof typeof state.errors] : undefined;
                    return (
                        <div className='flex flex-col items-start w-full' key={input.id}>
                            <Input {...input}/>
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
                <button disabled={pending} type="submit" className="bg-amber-300 p-3 rounded hover:bg-amber-500 text-amber-800">Login
                </button>
            </form>
            <ToastContainer/>
        </div>
    )
}

export default LoginForm;