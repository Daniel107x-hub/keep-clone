'use client'
import { signup } from '@/app/actions/auth';
import {ReactElement, useActionState} from "react";

const InputField = ({children}: {children: ReactElement}) => {
    return <div className="flex flex-col">
        {children}
    </div>
}

// Auth steps:
// 1. Create form -> ui/SignupForm
// 2. Validate fields on both client and server -> lib/definitions/SignupFormSchema
// 3. Execute registration -> actions/auth/signup

const SignupForm = () => {
    const [state, action, pending] = useActionState(signup, undefined);
    return (
        <form action={action} className='flex flex-col gap-4 items-center py-4'>
            <InputField>
                <>
                    <label htmlFor="firstName">First Name</label>
                    <input id="firstName" name="firstName" placeholder="First Name" />
                </>
            </InputField>
            {state?.errors?.firstName && <p>{state.errors.firstName}</p>}
            <InputField>
                <>
                    <label htmlFor="lastName">Last Name</label>
                    <input id="lastName" name="lastName" placeholder="Last Name" />
                </>
            </InputField>
            {state?.errors?.lastName && <p>{state.errors.lastName}</p>}
            <InputField>
                <>
                    <label htmlFor="email">Email</label>
                    <input id="email" name="email" placeholder="Email" />
                </>
            </InputField>
            {state?.errors?.email && <p>{state.errors.email}</p>}
            <InputField>
                <>
                    <label htmlFor="password">Password</label>
                    <input id="password" name="password" type="password" placeholder="Password" />
                </>
            </InputField>
            {state?.errors?.password && (
                <div>
                    <p>Password must:</p>
                    <ul>
                        {state.errors.password.map((error) => (
                            <li key={error}>- {error}</li>
                        ))}
                    </ul>
                </div>
            )}
            <button disabled={pending} type="submit" className="bg-amber-300 p-3 rounded hover:bg-amber-500">Sign up</button>
        </form>
    )
}

export default SignupForm;