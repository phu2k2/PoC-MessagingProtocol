import { IClientOptions } from 'mqtt'
import { TOptions } from '../types'

export const qosOption: TOptions[] = [
  {
    label: '0',
    value: 0,
  },
  {
    label: '1',
    value: 1,
  },
  {
    label: '2',
    value: 2,
  },
]

export const initialConnectionOptions: IClientOptions = {
  // ws or wss
  protocol: 'ws',
  host: 'localhost',
  clientId: 'mqtt_client_react' + Math.random().toString(16).substring(2, 8),
  // ws -> 8083; wss -> 8084
  port: 5000,
  /**
   * By default, EMQX allows clients to connect without authentication.
   * https://docs.emqx.com/en/enterprise/v4.4/advanced/auth.html#anonymous-login
   */
  username: 'mqtt_client_react',
  password: 'mqtt_client_react',
}
