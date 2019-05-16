clc
clear all

tcpipServer = tcpip('0.0.0.0',55000,'NetworkRole','Server', 'INPUT', 1000000);

fopen(tcpipServer);

try
    while(1)
        imageDataLength = fread(tcpipServer, 1, 'uint32');
        imageData = uint8(fread(tcpipServer, imageDataLength, 'uint8'));

        imageData = reshape(imageData, 3, []);
        imageData = imageData.';
        imageData = reshape(imageData, 640, 480, []);
        imageData = imrotate(imageData, 90);

        imshow(imageData);
    end
catch e
    fprintf("%d -", e.identifier);
    fprintf("\n %s", e.message);
end

fclose(tcpipServer);