import { ICybermesService } from "./ICybermesService";

export interface ICybermesProxy {
    name : string
    managementAddress: string
    listeningAddress: string
    updateMode: boolean
    linkedApiMes: ICybermesService
}