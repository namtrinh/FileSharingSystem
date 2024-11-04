use khachsan;


DELIMITER $$

CREATE PROCEDURE book_room(
    IN p_user_id INT,
    IN p_room_id INT,
    IN p_check_in DATE,
    IN p_check_out DATE,
    IN p_order_id VARCHAR(150),
    IN p_trans_amt INT
)
BEGIN
    DECLARE v_count INT;

    -- Kiểm tra xem phòng có khả dụng không
    SELECT COUNT(*) INTO v_count 
    FROM booking_order 
    WHERE room_id = p_room_id 
    AND (
        (check_in < p_check_out AND check_out > p_check_in) OR 
        (check_in >= p_check_in AND check_out <= p_check_out)
    );

    -- Nếu phòng không khả dụng, thông báo lỗi
    IF v_count > 0 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT = 'Xin lỗi, phòng này đã được đặt trong khoảng thời gian này.';
    ELSE
        -- Nếu phòng khả dụng, thêm đặt phòng vào bảng booking_order
        INSERT INTO booking_order (user_id, room_id, check_in, check_out, order_id, trans_amt, booking_status)
        VALUES (p_user_id, p_room_id, p_check_in, p_check_out, p_order_id, p_trans_amt, 'pending');
    END IF;
END$$

DELIMITER ;
