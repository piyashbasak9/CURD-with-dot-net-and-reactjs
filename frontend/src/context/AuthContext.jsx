import { useCallback, useMemo, useState } from 'react'
import { AuthContext, TOKEN_STORAGE_KEY } from './authContext'
import { login as loginRequest } from '../services/authService'

export function AuthProvider({ children }) {
  const [accessToken, setAccessToken] = useState(
    () => localStorage.getItem(TOKEN_STORAGE_KEY)
  )

  const login = useCallback(async (credentials) => {
    const response = await loginRequest(credentials)

    if (!response?.accessToken) {
      throw new Error('The sign-in response did not include an access token.')
    }

    localStorage.setItem(TOKEN_STORAGE_KEY, response.accessToken)
    setAccessToken(response.accessToken)
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_STORAGE_KEY)
    setAccessToken(null)
  }, [])

  const value = useMemo(
    () => ({ accessToken, isAuthenticated: Boolean(accessToken), login, logout }),
    [accessToken, login, logout]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
