/*****************************************************************************
 * main.c
 *  Main application file for the IoT hardware drivers demo.
 *  This file initializes all the hardware drivers and demonstrates their
 *  functionality.
 *  Push button 2 on the shield during reset to enter continious sensor
 *  reading mode. Otherwise the program will run an interactive demo that
 *  allows you to test each driver individually by sending commands over UART.
 *  See interactive.c for details.
 * 
 *  Author:  Erland Larsen
 *  Date:    2026-03-17
 *  Project: SPE4_API
 *****************************************************************************/
#include <avr/io.h>
#include <util/delay.h>
#include <avr/interrupt.h>
#include <stdio.h>
#include <string.h>
#include <stdint.h>

#include "uart_stdio.h"
#include "led.h"
#include "display.h"
#include "wifi.h"
#include "dht11.h"
#include "light.h"

#define INPUT_SIZE 64
#define TCP_RX_BUFFER_SIZE 128
#define TCP_PORT 5000

static uint8_t humidity_integer;
static uint8_t humidity_decimal;
static uint8_t temperature_integer;
static uint8_t temperature_decimal;

static char tcp_rx_buffer[TCP_RX_BUFFER_SIZE] = {0};

// remove trailing \r or \n from fgets input */
static void strip_newline(char *str)
{
    str[strcspn(str, "\r\n")] = '\0';
}

// called when TCP data is received from the server, used in case we want to do something with it in the future
void tcp_received_callback(void)
{
    printf("TCP RX: %s\n", tcp_rx_buffer);
}

// setup ESP8266 and open TCP connection to C# server
static uint8_t setup_wifi_and_tcp(void)
{
    char ssid[INPUT_SIZE];
    char password[INPUT_SIZE];
    char server_ip[INPUT_SIZE];

    printf("Checking ESP8266...\n");
    if (wifi_command_AT() != WIFI_OK)
    {
        printf("ESP8266 did not respond to AT command.\n");
        return 0;
    }
    printf("ESP8266 OK.\n");

    if (wifi_command_disable_echo() != WIFI_OK)
    {
        printf("Warning: failed to disable echo.\n");
    }

    if (wifi_command_set_mode_to_1() != WIFI_OK)
    {
        printf("Failed to set WiFi mode to station mode.\n");
        return 0;
    }

    if (wifi_command_set_to_single_Connection() != WIFI_OK)
    {
        printf("Failed to set single connection mode.\n");
        return 0;
    }

    printf("Enter WiFi SSID: ");
    fgets(ssid, sizeof(ssid), stdin);
    strip_newline(ssid);

    printf("Enter WiFi password: ");
    fgets(password, sizeof(password), stdin);
    strip_newline(password);

    printf("Enter C# server IP: ");
    fgets(server_ip, sizeof(server_ip), stdin);
    strip_newline(server_ip);

    printf("Joining WiFi...\n");
    if (wifi_command_join_AP(ssid, password) != WIFI_OK)
    {
        printf("Failed to join WiFi.\n");
        return 0;
    }
    printf("WiFi connected.\n");

    // close incace old one exists
    wifi_command_close_TCP_connection();

    printf("Opening TCP connection to %s:%u ...\n", server_ip, TCP_PORT);
    if (wifi_command_create_TCP_connection(server_ip, TCP_PORT,
                                           tcp_received_callback,
                                           tcp_rx_buffer) != WIFI_OK)
    {
        printf("Failed to create TCP connection.\n");
        return 0;
    }

    printf("TCP connected.\n");
    return 1;
}

// Json formatter
static void send_sensor_json(uint8_t t_i, uint8_t t_d,
                             uint8_t h_i, uint8_t h_d,
                             uint16_t light)
{
    char json[128];

    sprintf(json,
            "{\"temperature\":%d.%d,\"humidity\":%d.%d,\"light\":%u}\n",
            t_i, t_d, h_i, h_d, light);

    if (wifi_command_TCP_transmit((uint8_t *)json, strlen(json)) == WIFI_OK)
    {
        printf("TCP TX: %s", json);
    }
    else
    {
        printf("TCP send failed.\n");
    }
}

int main(void)
{
    led_init();
    display_init();
    light_init();
    wifi_init();

    if (UART_OK != uart_stdio_init(115200))
    {
        led_on(4);
        while (1)
            ;
    }

    sei();

    printf("SEP4 IoT sensor node booting...\n");

    if (!setup_wifi_and_tcp())
    {
        printf("WiFi/TCP setup failed. Program stopped.\n");
        led_on(4);
        while (1)
            ;
    }

    while (1)
    {
        DHT11_ERROR_MESSAGE_t error;
        uint16_t light;

        error = dht11_get(&humidity_integer, &humidity_decimal,
                          &temperature_integer, &temperature_decimal);

        light = light_measure_raw();

        if (error == DHT11_OK)
        {
            printf("Temperature: %d.%d C, Humidity: %d.%d %%, Light: %u\n",
                   temperature_integer,
                   temperature_decimal,
                   humidity_integer,
                   humidity_decimal,
                   light);

            //adjusting decimals
            display_setDecimals(1);
            display_int(temperature_integer * 10 + temperature_decimal);

            send_sensor_json(temperature_integer, temperature_decimal,
                             humidity_integer, humidity_decimal,
                             light);
        }
        else
        {
            printf("Failed to read DHT11. Light: %u\n", light);
        }

        _delay_ms(900000); //900000
    }
}