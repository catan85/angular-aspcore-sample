export interface ICybermesService {
    name : string
    version: string
    path: string
    isRunning: boolean
    hasLogFiles: boolean
    containsException: boolean
}