import { z } from 'zod';
import {NoteType} from "@/app/types/NoteType";

export const SignupFormSchema = z.object({
    firstName: z
        .string()
        .min(2, {message: 'First name is required'})
        .trim(),
    lastName: z
        .string()
        .min(2,  {message: 'Last name is required'})
        .trim(),
    userName: z
        .string()
        .min(4, {message: 'Username is required'})
        .trim(),
    phoneNumber: z
        .string()
        .min(10, {message: 'Phone number is required'})
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

export type SignupFormState = | {
    errors? : {
        firstName?: string[]
        lastName?: string[]
        userName?: string[]
        phoneNumber?: string[]
        email?: string[]
        password?: string[]
        request?: string
    }
    message?: string,
    payload?: object
} | undefined

export const LoginFormSchema = z.object({
    userName: z
        .string()
        .min(4, {message: 'Username is required'})
        .trim(),
    password: z
        .string()
        .min(8, {message: 'Password should have a min length of 8'})
        .trim()
})

export type LoginFormState = | {
    errors? : {
        userName?: string[]
        password?: string[]
        request?: string
    }
    message?: string,
    payload?: object
} | undefined;

export type NewNoteFormSate = | {
    errors? : {
        request?: string
    }
    message?: string
    data?: NoteType
} | undefined;