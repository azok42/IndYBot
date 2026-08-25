#!/bin/bash

set -e

SERVICE_NAME="indy-bot"
WORKING_DIR="$(pwd)"

EXECUTABLE=$(find "$WORKING_DIR" -maxdepth 1 -type f -executable | head -n 1)

if [ -z "$WORKING_DIR" ]; then
   echo "ERROR: No executable found"
   echo "You need to run the script"
   echo "in the directory where the"
   echo "executeable is located"
   exit 1
fi

SERVICE_USER="${SUDO_USER:-USER}"
SERVICE_FILE="$SERVICE_NAME.service"

echo "Creating systemd service..."

cat > "$SERVICE_FILE" << EOF
[Unit]
Description=IndYBot
After=network.target

[Service]
Type=simple
User=$SERVICE_USER
WorkingDirectory=$WORKING_DIR
ExecStart=$EXECUTABLE

Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
EOF

echo "Installing service..."

sudo cp "$SERVICE_FILE" "/etc/systemd/system/$SERVICE_FILE"

sudo systemctl daemon-reload
sudo systemctl enable "$SERVICE_FILE"

echo
echo "Service installed and enabled successfully."
echo
echo "Start service now"
echo "   sudo systemctl start $SERVICE_NAME"
echo
echo "Check status with:"
echo "   sudo systemctl status $SERVICE_NAME"
echo
echo "View logs with:"
echo "   sudo journalctl -u $SERVICE_NAME -f"
