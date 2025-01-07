import { z } from 'zod';

export const SignupFormSchema = z.object({
    firstName: z
        .string()
        .min(2, {message: 'First name is required'})
        .trim(),
    lastName: z
        .string()
        .min(2,  {message: 'Last name is required'})
        .trim(),
    email: z
        .string()
        .email({message: 'Email is required'})
        .trim(),
    password: z
        .string()
        .min(8, {message: 'Password should have a min length of 8'})
        .regex(/[a-zA-Z]/, {message: 'Password should contain at least one letter'})
        .regex(/[0-9]/, {message: 'Password should contain a least one number'})
        .regex(/[^a-zA-Z0-9]/, {message: 'Password should contain at least one special character'})
        .trim()
})

export type FormState = | {
    errors? : {
        firstName?: string[]
        lastName?: string[]
        email?: string[]
        password?: string[]
    }
    message?: string
} | undefined