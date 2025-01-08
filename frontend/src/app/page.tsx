"use client"

import useAuth from "@/app/hooks/useAuth";
import {redirect} from "next/navigation";

export default function Home() {
  const [isAuthenticated, isLoading] = useAuth();
  if(isLoading) return <div>Loading...</div>
  if(!isAuthenticated) redirect("/Login");
  return (
      <>Hello</>
  );
}