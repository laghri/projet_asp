import axios from "axios";
import { createContext, useContext, useEffect, useState } from "react";
import { getUrl } from '../API'
import { toast } from 'react-toastify';
import { useNavigate } from "react-router-dom";


const authContext = createContext();

export const useAuth = () => {
    const context = useContext(authContext);
    if (!context) throw new Error("Auth Provider is missing");
    return context;
};

const authUrl = getUrl('Auth')



export const AuthContextProvider = ({ children }) => {
    
    const navigate = useNavigate()
    const [loading,setLoading] = useState(false)
    const [userInfo,setUserInfo] = useState(null)
    
    
    const login = async(username,password) => {
        setLoading(true)
        try {
            const config = {
                headers: {
                    'Content-Type': 'application/json',
                },
            }

            const { data } = await axios.post(
                `${authUrl}/login`,
                { username, password },
                config
            )
            setUserInfo(data)
            localStorage.setItem('userInfo',JSON.stringify(data))
            return data
        } catch (error) {
            toast.error("An error Occured")
            console.log(error)
        }
    }

    const register = async(email,username,firstName,lastName,password,role) => {
        setLoading(true)
        try {
            const config = {
                headers: {
                    'Content-Type': 'application/json',
                },
            }

            const { data } = await axios.post(
                `${authUrl}/register`,
                { email,username,nom:firstName,prenom:lastName, password,role },
                config
            )
            console.log(data)
            setUserInfo(data)
            localStorage.setItem('userInfo',JSON.stringify(data))
            return data
        } catch (error) {
            toast.error("An error Occured")
            console.log(error)
        }
    }



    return (
        <authContext.Provider
            value={{
               userInfo,
               loading,
               setLoading,
               login,
               register,
            }}
        >
            {children}
        </authContext.Provider>
    );
};